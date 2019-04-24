using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System;
using System.Collections;

public class InscriptionScript : MonoBehaviour
{
    public Text participantsNumberText;
    public Text errorText;
    public Transform inviteSection;
    public int maxParticipants;
    public TextAsset credentialsFile;

    private int mParticipantsNumber = 1;
    private Stack<Transform> mInviteSectionStack = new Stack<Transform>();
    private Transform mInviteSectionTemplate;
    private Transform mInviteContainer;
    private Transform mConfirmationPanel;
    private Transform mInscriptionPanel;


    protected void Awake()
    {
        mConfirmationPanel = transform.Find("ConfirmationPanel");
        mInscriptionPanel = transform.Find("InscriptionPanel");

        checkIfSubscribed();
    }

    protected void Start()
    {
        if (!PersistentToken.hasAccessToMeal()) inviteSection.FindDeepChild("Menu").gameObject.SetActive(false);

        mInviteSectionTemplate = Instantiate(inviteSection);
        mInviteSectionStack.Push(inviteSection);
        mInviteContainer = inviteSection.parent;
    }

    public void AddParticipant()
    {
        if (mParticipantsNumber < maxParticipants)
        {
            mParticipantsNumber++;
            participantsNumberText.text = mParticipantsNumber.ToString();
            Transform newInvite = Instantiate(mInviteSectionTemplate, mInviteContainer);

            newInvite.FindDeepChild("InviteTitle").GetComponent<Text>().text = mParticipantsNumber + "e invité";

            mInviteSectionStack.Push(newInvite);
        }
    }

    public void SubParticipant()
    {
        if (mParticipantsNumber > 1)
        {
            mParticipantsNumber--;
            participantsNumberText.text = mParticipantsNumber.ToString();
            Destroy(mInviteSectionStack.Pop().gameObject);
        }
    }

    public void Transmit()
    {
        try
        {
            string message = buildMail();

            sendMail("nboder@gmail.com", "Nouvelle inscription au mariage", message);

            PersistentToken.SetRegistration(true);
            checkIfSubscribed();
        }
        catch (MissingFieldException e)
        {
            errorText.text = e.Message;
            StartCoroutine(ResetError(1));
        }
        catch (SmtpException)
        {
            errorText.text = "Echec de l'envoi";
            StartCoroutine(ResetError(1));
        }
    }

    private string buildMail()
    {
        string message = "<div>De nouvelles personnes se sont inscrits au mariage (<span style=\"color:red;\">";
        message += PersistentToken.hasAccessToMeal() ? "souper" : "apero";
        message += "</span>)</div>";

        foreach (Transform invite in mInviteContainer)
        {
            message += "<div style=\"color:red;\">";
            string prenom = invite.FindObjectsWithTag("PrenomField").LastOrDefault().GetComponent<Text>().text;
            string nom = invite.FindObjectsWithTag("NomField").LastOrDefault().GetComponent<Text>().text;
            if (prenom == "" || nom == "")
            {
                throw new MissingFieldException("Prénom et Nom requis");
            }

            message = string.Concat(message, prenom, " ");
            message = string.Concat(message, nom, "</div>");

            if (PersistentToken.hasAccessToMeal())
            {
                message = string.Concat(message, "<div style=\"color:red;\">", invite.FindObjectsWithTag("MenuToggle").LastOrDefault().GetComponent<ToggleGroup>().ActiveToggles().First().GetComponentInChildren<Text>().text, "</div>");
            }
        }

        message = string.Concat(message, "<div>Remarques: <span style=\"color:red;\">", transform.FindObjectsWithTag("RemarquesField").LastOrDefault().GetComponent<Text>().text, "</span></div>");

        return message;
    }

    private void sendMail(string recipient, string subject, string body)
    {
        MailMessage mail = new MailMessage();

        Dictionary<string, string> credentials = CredentialsHelper.GetDictionary(credentialsFile);

        mail.From = new MailAddress(credentials["mail"]);
        mail.To.Add(recipient);
        mail.Subject = subject;
        mail.Body = body;
        mail.BodyEncoding = System.Text.Encoding.UTF8;
        mail.IsBodyHtml = true;

        SmtpClient smtpServer = new SmtpClient(credentials["server"]);
        smtpServer.Port = 587;
        smtpServer.Credentials = new System.Net.NetworkCredential(credentials["mail"], credentials["password"]) as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        smtpServer.Send(mail);
        Debug.Log("success");
    }

    private IEnumerator ResetError(int time)
    {
        yield return new WaitForSeconds(time);
        errorText.text = "";
    }

    private void checkIfSubscribed()
    {
        if (PersistentToken.IsRegistred())
        {
            mConfirmationPanel.gameObject.SetActive(true);
            mInscriptionPanel.gameObject.SetActive(false);
        }
    }
}
