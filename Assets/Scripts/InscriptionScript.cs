using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System;

public class InscriptionScript : MonoBehaviour
{
    public Text participantsNumberText;
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
        m_hasAccessToMeal = (0 != PlayerPrefs.GetInt("souper", 0));

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

            PlayerPrefs.SetInt("registred", 1);

            checkIfSubscribed();
        }
        catch (MissingFieldException e)
        {
            Debug.Log(e);
            AlertPrefab.LaunchAlert(e.Message);
        }
        catch (SmtpException e)
        {
            AlertPrefab.LaunchAlert("Echec de l'envoi (" + e.StatusCode.ToString() + ")");
        }
    }

    private string buildMail()
    {

        string message = "<!DOCTYPE html><html lang='fr'><head><meta charset='utf-8'><meta name='viewport' content='width=device-width'><meta http-equiv='X-UA-Compatible' content='IE=edge'><meta name='x-apple-disable-message-reformatting'><title>I&N</title><style> span {color : red; }</style></head><body><p>De nouvelles personnes se sont inscrites au mariage (accès <span style=\'color:red;\'>";
        message += m_hasAccessToMeal ? "souper" : "apéro";
        message += "</span>)</p>";

        foreach (Transform invite in mInviteContainer)
        {
            bool participe = invite.FindObjectsWithTag("PresenceToggle").LastOrDefault().GetComponentInChildren<Toggle>().isOn;

            string prenom = invite.FindObjectsWithTag("PrenomField").LastOrDefault().GetComponent<Text>().text;
            string nom = invite.FindObjectsWithTag("NomField").LastOrDefault().GetComponent<Text>().text;
            if (prenom == "" || nom == "")
            {
                throw new MissingFieldException("Prénom et Nom requis");
            }

            message = string.Concat(message, "<p><b>", participe ? "Vient au mariage" : "Ne vient pas au mariage", "</b><br>");
            message += "<span>";
            message = string.Concat(message, prenom, " ");
            message = string.Concat(message, nom, "</span><br>");

            if (participe && m_hasAccessToMeal)
            {
                message = string.Concat(message, "<span>", invite.FindObjectsWithTag("MenuToggle").LastOrDefault().GetComponent<ToggleGroup>().ActiveToggles().First().GetComponentInChildren<Text>().text, "</span>");
            }

            message = string.Concat(message, "</p>");
        }

        string email = transform.FindObjectsWithTag("EmailField").LastOrDefault().GetComponent<Text>().text;
        if (email == "")
        {
            throw new MissingFieldException("Email requis");
        }
        message = string.Concat(message, "<p>Email: ", email, "</p>");

        string remarques = transform.FindObjectsWithTag("RemarquesField").LastOrDefault().GetComponent<Text>().text;
        message = string.Concat(message, "<p>Remarques: <span>", remarques, "</span></p>");
        message = string.Concat(message, "</body></html>");

        return message;
    }

    private void sendMail(string recipient, string subject, string body)
    {
        AlertPrefab.LaunchAlert("Inscription en cours d'envoi.");
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

    private void checkIfSubscribed()
    {
        bool registred = (0 != PlayerPrefs.GetInt("registred", 0));
        mConfirmationPanel.gameObject.SetActive(registred);
        mInscriptionPanel.gameObject.SetActive(!registred);
    }
}
