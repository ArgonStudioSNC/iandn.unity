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
    private bool m_hasAccessToMeal;

    protected void OnEnable()
    {
        checkIfSubscribed();
    }

    protected void Awake()
    {
        mConfirmationPanel = transform.Find("ConfirmationPanel");
        mInscriptionPanel = transform.Find("InscriptionPanel");
    }

    protected void Start()
    {
        m_hasAccessToMeal = PersistentToken.hasAccessToMeal();

        if (!m_hasAccessToMeal) inviteSection.FindDeepChild("Menu").gameObject.SetActive(false);
        PresenseStatusChanged(inviteSection);

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

    public void PresenseStatusChanged(Transform invite)
    {
        if (m_hasAccessToMeal)
        {
            bool comming = invite.FindObjectsWithTag("PresenceToggle").LastOrDefault().GetComponentInChildren<Toggle>().isOn;
            invite.FindDeepChild("MenuMask").GetComponent<Image>().enabled = !comming;
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
            Debug.Log(e);
            errorText.text = e.Message;
            StartCoroutine(ResetError(2));
        }
        catch (SmtpException e)
        {
            errorText.text = "Echec de l'envoi (" + e.StatusCode.ToString() + ")";
            StartCoroutine(ResetError(2));
        }
    }

    private string buildMail()
    {
        string message = "<div>De nouvelles personnes se sont inscrits au mariage (acces <span style=\"color:red;\">";
        message += m_hasAccessToMeal ? "souper" : "apero";
        message += "</span>)</div>";

        foreach (Transform invite in mInviteContainer)
        {
            bool participe = invite.FindObjectsWithTag("PresenceToggle").LastOrDefault().GetComponentInChildren<Toggle>().isOn;

            string prenom = invite.FindObjectsWithTag("PrenomField").LastOrDefault().GetComponent<Text>().text;
            string nom = invite.FindObjectsWithTag("NomField").LastOrDefault().GetComponent<Text>().text;
            if (prenom == "" || nom == "")
            {
                throw new MissingFieldException("Prénom et Nom requis");
            }

            message = string.Concat(message, "<p><div><b>", participe ? "Vient au mariage" : "Ne vient pas au mariage", "</b></div>");
            message += "<div style=\"color:red;\">";
            message = string.Concat(message, prenom, " ");
            message = string.Concat(message, nom, "</div>");

            if (participe && m_hasAccessToMeal)
            {
                message = string.Concat(message, "<div style=\"color:red;\">", invite.FindObjectsWithTag("MenuToggle").LastOrDefault().GetComponent<ToggleGroup>().ActiveToggles().First().GetComponentInChildren<Text>().text, "</div>");
            }

            message = string.Concat(message, "</p>");
        }

        message = string.Concat(message, "<p><div>Remarques: <span style=\"color:red;\">", transform.FindObjectsWithTag("RemarquesField").LastOrDefault().GetComponent<Text>().text, "</span></div></p>");

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
        mConfirmationPanel.gameObject.SetActive(PersistentToken.IsRegistred());
        mInscriptionPanel.gameObject.SetActive(!PersistentToken.IsRegistred());
    }
}
