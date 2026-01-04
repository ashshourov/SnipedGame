using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;

public class LoginManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField usernameInput;

    [Header("Buttons")]
    public Button guestButton;
    public Button signInButton;
    public Button registerButton;

    [Header("UI Feedback")]
    public TextMeshProUGUI feedbackText;

    [Header("Colors")]
    public Color loadingColor = Color.yellow;
    public Color successColor = Color.green;
    public Color errorColor = Color.red;

    [Header("Scene Settings")]
    public string gameSceneName = "FaceTracking 1";

    private bool isProcessing = false;

    void Start()
    {
        if (guestButton != null)
            guestButton.onClick.AddListener(OnGuestLoginClicked);
        if (signInButton != null)
            signInButton.onClick.AddListener(OnSignInClicked);
        if (registerButton != null)
            registerButton.onClick.AddListener(OnRegisterClicked);

        ClearFeedback();
    }

    public async void OnGuestLoginClicked()
    {
        if (isProcessing) return;

        isProcessing = true;
        SetButtonsInteractable(false);

        SetFeedbackText("Signing in as guest...", loadingColor);

        bool success = await PlayFabManager.Instance.SignInGuest();

        if (this == null) return;

        if (success)
        {
            SetFeedbackText("Guest Login Successful!", successColor);
            await Task.Delay(1000);
            LoadGameScene();
        }
        else
        {
            SetFeedbackText("Guest login failed. Check connection.", errorColor);
            SetButtonsInteractable(true);
            isProcessing = false;
        }
    }

    public async void OnSignInClicked()
    {
        if (isProcessing) return;

        if (string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            SetFeedbackText("Please enter both email and password.", errorColor);
            return;
        }

        if (!IsValidEmail(emailInput.text))
        {
            SetFeedbackText("Please enter a valid email address.", errorColor);
            return;
        }

        isProcessing = true;
        SetButtonsInteractable(false);

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        SetFeedbackText("Signing in...", loadingColor);

        bool success = await PlayFabManager.Instance.SignInUser(email, password);

        if (this == null) return;

        if (success)
        {
            SetFeedbackText("Sign In Successful!", successColor);
            await Task.Delay(1000);
            LoadGameScene();
        }
        else
        {
            SetFeedbackText("Sign in failed. Check your credentials.", errorColor);
            SetButtonsInteractable(true);
            isProcessing = false;
        }
    }

    public async void OnRegisterClicked()
    {
        if (isProcessing) return;

        if (string.IsNullOrEmpty(emailInput.text) ||
            string.IsNullOrEmpty(passwordInput.text) ||
            string.IsNullOrEmpty(usernameInput.text))
        {
            SetFeedbackText("Please fill in all fields.", errorColor);
            return;
        }

        if (!IsValidEmail(emailInput.text))
        {
            SetFeedbackText("Please enter a valid email address.", errorColor);
            return;
        }

        if (passwordInput.text.Length < 6)
        {
            SetFeedbackText("Password must be at least 6 characters.", errorColor);
            return;
        }

        if (usernameInput.text.Length < 3)
        {
            SetFeedbackText("Username must be at least 3 characters.", errorColor);
            return;
        }

        isProcessing = true;
        SetButtonsInteractable(false);

        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string username = usernameInput.text.Trim();

        SetFeedbackText("Registering new user...", loadingColor);

        bool success = await PlayFabManager.Instance.RegisterUser(email, password, username);

        if (this == null) return;

        if (success)
        {
            SetFeedbackText("Registration Successful!", successColor);
            await Task.Delay(1000);
            LoadGameScene();
        }
        else
        {
            SetFeedbackText("Registration failed. Email may already be in use.", errorColor);
            SetButtonsInteractable(true);
            isProcessing = false;
        }
    }

    private void SetFeedbackText(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
        Debug.Log($"Login Flow: {message}");
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (guestButton != null) guestButton.interactable = interactable;
        if (signInButton != null) signInButton.interactable = interactable;
        if (registerButton != null) registerButton.interactable = interactable;
    }

    private void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Game scene name is not set!");
        }
    }

    private bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
}