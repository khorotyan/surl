using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RefVars : MonoBehaviour
{
    [Header("Signup Panel")]
    [Space(5)]
    public GameObject signupPanel;
    public InputField suUsernameIn;
    public InputField suEmailIn;
    public InputField suPassIn;
    public InputField suConfPassIn;
    public Button suSignupB;
    public Button suSigninB;

    [Header("Signin Panel")]
    [Space(5)]
    public GameObject signinPanel;
    public InputField siEmailIn;
    public InputField siPassIn;
    public Button siSigninB;
    public Button siSignupB;
    public Button siSkipB;

    [Header("Question Answer Panel")]
    [Space(5)]
    public GameObject qaPanel;
    public Button qaHomeB;
    public Button qaUsersB;
    public Button qaMoreB;
    public GameObject qaMoreOptions;
    public Button qaNewQuestB;
    public InputField qaSearchIn;
    public Button qaSearchClearB;
    public Button qaSortB;
    public GameObject qaSortOptions;
    public Transform qaQuestCont;
    public Transform qaQuestParent;
    public GameObject questionObj;
    public Button qaLoginB;
    public Button qaFollowersB;
    public Button qaFollowingB;
    public Button qaLogoutB;
    public Button qaMostPopB;
    public Button qaMostRecB;
    public Button qaMyNetworkB;
    public Button qaMyQuestB;
    public Button qaTrendingB;
    public Button qaUnansB;
    public Button qaUnverifB;

    [Header("Add Question Panel")]
    [Space(5)]
    public GameObject addQPanel;
    public Button aqBackB;
    public Text aqUsernameT;
    public InputField aqQuestIn;
    public Text aqQuestLimitT;
    public InputField aqDescIn;
    public Text aqDescLimitT;
    public InputField aqTagsIn;
    public Text aqTagsLimitT;
    public Button aqPostB;

    [Header("Users Panel")]
    [Space(5)]
    public GameObject usersPanel;
    public Button usBackB;
    public Text usBarTitleT;
    public Button usSearchB;
    public InputField usSearchIn;
    public Button usSearchClearB;
    public Transform usUsersCont;
    public Transform usUsersParent;
    public GameObject userObj;
    public GameObject userNTimeObj;

    [Header("Question Comment Panel")]
    [Space(5)]
    public GameObject qcPanel;
    public Button qcBackB;
    public Button qcAddCommB;
    public Text qcQuestVotesT;
    public Button qcUpVoteB;
    public Button qcDownVoteB;
    public Text qcQuestT;
    public Transform qcTagsCont;
    public Transform qcTagsParent;
    public GameObject tagObj;
    public Text qcDescT;
    public Text qcPublisherInfoT;
    public Button qcSortB;
    public GameObject qcSortOptions;
    public Text qcViewsT;
    public Text qcAnswersT;
    public GameObject commentObj;
    public Transform qcCommentsCont;
    public Transform qcCommentsParent;
    public Button qcVotesB;
    public Button qcDateB;

    [Header("Other")]
    [Space(5)]
    public Transform infoBox;
    public Text infoText;

    [System.NonSerialized]
    public string reqURL;

    // Info Shower 
    private float infoTime = 0f;
    private bool canShowInfo = false;
    private bool canShowURlPanel = false;
    private string info = "";

    private void Awake()
    {
        Application.runInBackground = true;

        reqURL = "http://localhost:49658";
    }

    private void Update()
    {
        SendInfo();
    }

    public void ShowInfo(string info)
    {
        this.info = info;
        infoTime = 0;
        canShowInfo = true;
    }
    
    // Shows user warnings
    private void SendInfo()
    {
        if (canShowInfo == true)
        {
            if (infoTime == 0)
            {
                infoBox.GetComponent<RectTransform>().DOAnchorPosY(0f, 0.6f).SetEase(Ease.OutExpo);
                infoText.text = info;
            }

            infoTime += Time.deltaTime;

            if (infoTime > 2f)
            {
                infoBox.GetComponent<RectTransform>().DOAnchorPosY(-26, 0.6f).SetEase(Ease.OutExpo);
                canShowInfo = false;
            }
        }
    } 
}
