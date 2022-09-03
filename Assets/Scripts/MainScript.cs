using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
//using UnityEngine.SocialPlatforms.Impl;

public class MainScript : MonoBehaviour
{
    public GameObject CanvasObject;
    public GameObject BodyGridBoxObject;
    public GameObject UpperObject;
    public GameObject FooterObject;
    public GameObject BackgroundObject;
    public GameObject MainLayoutObject;
    public GameObject QNABodyBoxObject;
    public GameObject QNALayoutObject;
    public GameObject QNAUpperObject;
    public GameObject QNAFooterObject;
    public GameObject SoundObject;
    public GameObject QuestionImageBoxObject;
    public GameObject QuestionTextObject;
    public GameObject QuestionTextBoxObject;
    public GameObject QuestionImageObject;
    public GameObject BokPanelObject;
    public GameObject HelperObject;
    public GameObject[] HelperAnsBtnObjects = new GameObject[2];
    public GameObject BountyTextObject;
    public GameObject ScoreTextObject;
    public GameObject NextBtn;
    public GameObject SettingsBoxObject;
    public GameObject VolumeSliderObject;
    public GameObject ChekerSoundBtnObject;
    public GameObject OutFromSettingsBtnObject;
    public GameObject[] CellsInBodyGridObject = new GameObject[16];
    public GameObject[] AnswerButtonObjects = new GameObject[4];
    public Slider SliderTimer;
    public Sprite GreenBtn;
    public Sprite BlueBtn;
    public Sprite RedBtn;
    public Sprite OrangeBtn;
    public Sprite RedDoorBtn;
    public Sprite GreenDoorBtn;
    public Sprite NormalAnswerBtn;
    public Sprite RedAnswerBtn;
    public Sprite GreenAnswerBtn;
    public Sprite[] SpritesForHelp = new Sprite[5];
    public int PriceToOpenMap = 20;
    public int PriceToOpenExit = 10;
    public int UsHelperActive = 1;
    public List<QuestionDataModel> FullListQuestionAndAnswers;
    public AudioClip OpenCellSound;
    public AudioClip BackgroundSound;
    public AudioClip TicTacSound;
    public AudioClip NegativeAnswerSound;
    public AudioClip PositiveAnswerSound;
    public AudioClip ShelkSound;

    private QandA[] FullList;
    private float TimerCounter;
    private float UsVolume;
    private int NowQNum;
    private int NowCell;
    private int UsTotalScore;
    private int UsTotalBounty;
    private int[] StartMask = { -1, -1, -1, -1, -1, -1, -5, -1, -1, -1, -1, -1, -2, -1, -1, -1 };
    private int[] StartMap = { 9, 10, 11, 12, 0, 7, 8, 0, 0, 5, 6, 0, 1, 2, 3, 4 };
    private MapNMask[] FullMap = new MapNMask[16];
    private int IndexOfRightAnswer;
    Sprite ActiveSprite;
    private string PathToJSONQNA;
    private string PathToInfo;
    private string PathToSCVMap;
    private string PathToSCVMSK;
    private bool CoroutineIsRunning = false;
    private bool TimerCheker = false;
    private bool UsVolumeChek;
    private bool SettingsOpened = false;
    private bool MainMenuOpened = false;
    private bool BokPanelOpened = false;
    private bool CellOpened = false;
    private bool InformOpened = false;
    private UnityAction[] BtnsAction = new UnityAction[4];
    void Start()
    {

        PathToJSONQNA = "Map1Data/ow";
        FullListQuestionAndAnswers = ReadJSONFile(PathToJSONQNA);
        PathToInfo = Application.persistentDataPath + "/InfoPersonal.json";
        PathToSCVMap = Application.persistentDataPath + "/map.csv";
        PathToSCVMSK = Application.persistentDataPath + "/mapmask.csv";
        string TestPath = Application.persistentDataPath + "/Out.csv";
        CheckFiles();
        int[] testint = { 1, 2, 3, 3, 5, 21, 3, 4, 5, 3, 1, 2, 3, 4, 5, 12 };
        StartCoroutine(InitMap());
    }
    public void CheckFiles()
    {
        if (!File.Exists(PathToInfo))
        {
            InfoPerson per1 = new InfoPerson();
            per1.TotalBounty = 0;
            per1.TotalScore = 0;
            per1.HelperActive = 1;
            per1.Volume = 0.3f;
            per1.VolumeChek = true;

            string json = JsonUtility.ToJson(per1);
            File.WriteAllText(PathToInfo, json);
            UsTotalBounty = 60;
            UsTotalScore = 60;
            UsHelperActive = 1;
            UsVolume = 0.3f;
            UsVolumeChek = true;
        }
        else
        {
            string json = System.IO.File.ReadAllText(PathToInfo);
            InfoPerson MyUser = JsonUtility.FromJson<InfoPerson>(json);
            UsTotalBounty = MyUser.TotalBounty;
            UsTotalScore = MyUser.TotalScore;
            UsHelperActive = MyUser.HelperActive;
            UsVolume = MyUser.Volume;
            UsVolumeChek = MyUser.VolumeChek;
        }
        if (!File.Exists(PathToSCVMap))
        {
            WritetoCSVMap(PathToSCVMap, StartMap);
            WritetoCSVMap(PathToSCVMSK, StartMask);
        }

    }

    public string SerializeToJson(List<QuestionDataModel> dataList)
    {
        ListContainer container = new ListContainer(dataList);
        string json = JsonUtility.ToJson(container);
        return json;
    }

    public List<QuestionDataModel> DeserializeFromJson(string json)
    {
        ListContainer container = JsonUtility.FromJson<ListContainer>(json);
        return container.dataList;
    }

    public struct QandA
    {
        public string Question;
        public string RightA;
        public string[] Answers;
        public string NameOfImage;
        public string TextWrong;
        public string Clarification;

    }
    public struct MapNMask
    {
        public int Map;
        public int Mask;
    }
    public struct AButtonLook
    {
        public int index;
        public string text;
    }
    IEnumerator InitMap()
    {

        MainMenuOpened = true;
        QNALayoutObject.active = false;
        MainLayoutObject.active = true;
        RectTransform rt = MainLayoutObject.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1080,CanvasObject.GetComponent<RectTransform>().rect.height);
        MainLayoutObject.active = true;
        string json = System.IO.File.ReadAllText(PathToInfo);
        InfoPerson MyUser = JsonUtility.FromJson<InfoPerson>(json);
        UsTotalBounty = MyUser.TotalBounty;
        UsTotalScore = MyUser.TotalScore;

        FullList = new QandA[FullListQuestionAndAnswers.Count];
        for (int i = 0; i < FullListQuestionAndAnswers.Count; i++)
        {
            FullList[i].Question = FullListQuestionAndAnswers[i].QuestionTS;
            FullList[i].Answers = FullListQuestionAndAnswers[i].Answers;
            FullList[i].RightA = FullListQuestionAndAnswers[i].RightAnswer;
            FullList[i].NameOfImage = FullListQuestionAndAnswers[i].PathToImage;
            FullList[i].Clarification = FullListQuestionAndAnswers[i].Clarification;
        }
        FullMap = ReadCSVMap(PathToSCVMap, PathToSCVMSK);
        BountyTextObject.GetComponent<TextMeshProUGUI>().text = UsTotalBounty.ToString();
        ScoreTextObject.GetComponent<TextMeshProUGUI>().text = UsTotalScore.ToString();
        FooterObject.gameObject.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = PriceToOpenExit.ToString();
        FooterObject.gameObject.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = PriceToOpenMap.ToString();
        if (UsTotalBounty >= PriceToOpenExit) FooterObject.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
        else FooterObject.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = false;
        if (UsTotalBounty >= PriceToOpenMap) FooterObject.gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
        else FooterObject.gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = false;

        for (int i = 0; i < 16; i++)
        {
            if (FullMap[i].Mask == -1)
            {   // closed cell 
                CellsInBodyGridObject[i].GetComponent<Button>().interactable = true;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.active = true;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = BlueBtn;
            }
            if (FullMap[i].Mask == -2 || FullMap[i].Mask == -8)
            {   // opened cell 
                CellsInBodyGridObject[i].GetComponent<Button>().interactable = true;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.active = true;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = OrangeBtn;
            }
            if (FullMap[i].Mask == -45)
            {   // opened door vernii otvet
                CellsInBodyGridObject[i].GetComponent<Button>().interactable = true;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.active = true;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = GreenDoorBtn;
            }
            if (FullMap[i].Mask == -4)
            {   // vernii otvet
                CellsInBodyGridObject[i].GetComponent<Button>().interactable = true;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.active = true;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = GreenBtn;
            }
            if (FullMap[i].Mask == -35)
            {   // opened door ne vernii otvet
                CellsInBodyGridObject[i].GetComponent<Button>().interactable = true;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.active = true;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = RedDoorBtn;
            }
            if (FullMap[i].Mask == -3)
            {   // ne vernii otvet
                CellsInBodyGridObject[i].GetComponent<Button>().interactable = true;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.active = true;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = RedBtn;
            }
            if (FullMap[i].Map == 0)
            {   // dont work
                CellsInBodyGridObject[i].GetComponent<Button>().interactable = false;
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.active = false;

            }
        }
        if (!UpperObject.gameObject.GetComponent<Animator>().enabled) UpperObject.gameObject.GetComponent<Animator>().enabled = true;
        else UpperObject.gameObject.GetComponent<Animator>().SetTrigger("On");
        if (!BodyGridBoxObject.gameObject.GetComponent<Animator>().enabled) BodyGridBoxObject.gameObject.GetComponent<Animator>().enabled = true;
        else BodyGridBoxObject.gameObject.GetComponent<Animator>().SetTrigger("GoAnim");
        if (!FooterObject.gameObject.GetComponent<Animator>().enabled) FooterObject.gameObject.GetComponent<Animator>().enabled = true;
        else FooterObject.gameObject.GetComponent<Animator>().SetTrigger("On");

        yield return new WaitForSeconds(0.25f);
        UpperObject.gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
        for (int i = 0; i < 16; i++)
            if (FullMap[i].Map != 0) CellsInBodyGridObject[i].GetComponent<Button>().interactable = true;
        ;
        BackgroundObject.GetComponent<AudioSource>().enabled = true;
        BackgroundObject.GetComponent<AudioSource>().loop = true;
        BackgroundObject.GetComponent<AudioSource>().clip = BackgroundSound;
        BackgroundObject.GetComponent<AudioSource>().volume = UsVolume;
        BackgroundObject.GetComponent<AudioSource>().mute = !UsVolumeChek;
        BackgroundObject.GetComponent<AudioSource>().Play();
        SoundObject.GetComponent<AudioSource>().mute = BackgroundObject.GetComponent<AudioSource>().mute;
        SoundObject.GetComponent<AudioSource>().volume = BackgroundObject.GetComponent<AudioSource>().volume;
        SoundObject.GetComponent<AudioSource>().Play();

        if (UsHelperActive == 1) StartCoroutine(StartHelp(0));
        /*  FooterObject.gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
          FooterObject.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
  */
    }

    IEnumerator StartHelp(int cadr)
    {

        HelperObject.GetComponent<Image>().sprite = SpritesForHelp[cadr];
        HelperObject.active = true;
        HelperAnsBtnObjects[0].active = true;
        HelperAnsBtnObjects[0].gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Хорошо, я понял!";
        if (cadr == 0)
        {
            HelperAnsBtnObjects[0].gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Да, конечно!";
            HelperAnsBtnObjects[1].active = true;
            HelperAnsBtnObjects[1].gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Нет, спасибо";
        }
        HelperObject.GetComponent<Animator>().enabled = true;
        HelperObject.GetComponent<Animator>().SetTrigger("GoShow");
        yield return new WaitForSeconds(0.25f);
        HelperAnsBtnObjects[0].GetComponent<Animator>().enabled = true;
        HelperAnsBtnObjects[0].GetComponent<Animator>().SetTrigger("Create");
        if (cadr == 0)
        {
            yield return new WaitForSeconds(0.1f);
            HelperAnsBtnObjects[1].GetComponent<Animator>().enabled = true;
            HelperAnsBtnObjects[1].GetComponent<Animator>().SetTrigger("Create");
        }
        yield return new WaitForSeconds(0.3f);
        HelperAnsBtnObjects[0].GetComponent<Button>().interactable = true;
        if (cadr == 0)
        {
            HelperAnsBtnObjects[1].GetComponent<Button>().interactable = true;
            HelperAnsBtnObjects[1].GetComponent<Button>().onClick.AddListener(delegate {HelperAnsBtnOnClick(-1); });
        }

        int temptoonclick = cadr;
        HelperAnsBtnObjects[0].GetComponent<Button>().onClick.RemoveAllListeners();
        HelperAnsBtnObjects[0].GetComponent<Button>().onClick.AddListener(delegate { HelperAnsBtnOnClick(temptoonclick);});
        if (cadr == 4) 
            while(true)
            {
                yield return new WaitForSeconds(0.1f);
                if (UsHelperActive == 0) yield return null;
            }
    }
    
    public MapNMask[] ReadCSVMap(string path, string path2)
    {
        MapNMask[] tempint = new MapNMask[16];
        string[] linesmap;
        string[] linesmask;
        string[] linesdatmap;
        string[] linesdatmask;
        string fileDatamap = System.IO.File.ReadAllText(path);
        string fileDatamask = System.IO.File.ReadAllText(path2);
        linesmap = fileDatamap.Split("\n"[0]);
        linesmask = fileDatamask.Split("\n"[0]);
        for (int i = 0; i < 4; i++)
        {
            linesdatmap = (linesmap[i].Trim()).Split(";"[0]);
            linesdatmask = (linesmask[i].Trim()).Split(";"[0]);
            for (int j = 0; j < 4; j++)
            {
                int count = i * 4 + j;
                tempint[i * 4 + j].Map = Int32.Parse(linesdatmap[j]);
                tempint[i * 4 + j].Mask = Int32.Parse(linesdatmask[j]);
            }
        }
        
        return tempint;
    }
    
    public List<QuestionDataModel> ReadJSONFile(string path)
    {
        TextAsset jsonTextFile = Resources.Load<TextAsset>(path);
        Debug.Log(System.Text.Encoding.UTF8.GetString(jsonTextFile.bytes));
        Debug.Log(jsonTextFile.ToString());
        return DeserializeFromJson(jsonTextFile.ToString());
    }
    public void OnClickBodyCell(int Num)
    {
        if (FullMap[Num].Mask == -45 || FullMap[Num].Mask == -35)
            StartCoroutine(OpenCellNQNA(Num, FullMap[Num].Map));
        if (FullMap[Num].Mask == -1 || FullMap[Num].Mask == -5)
            if (!CellsInBodyGridObject[Num].GetComponent<Animator>().enabled) CellsInBodyGridObject[Num].GetComponent<Animator>().enabled = true;
            else CellsInBodyGridObject[Num].GetComponent<Animator>().SetTrigger("Closed");
        if (FullMap[Num].Mask == -2 || FullMap[Num].Mask == -8) StartCoroutine(OpenCellNQNA(Num,FullMap[Num].Map));

    }
    public void OnClickVolumeCheker()
    {
        if (UsVolumeChek) UsVolumeChek = false;
        else UsVolumeChek = true;
        if (UsVolumeChek)
        {
            ChekerSoundBtnObject.GetComponent<Image>().sprite = GreenBtn;
            BackgroundObject.GetComponent<AudioSource>().mute = false;
        }
        else
        {
            ChekerSoundBtnObject.GetComponent<Image>().sprite = RedBtn;
            BackgroundObject.GetComponent<AudioSource>().mute = true;
        }

    }
    public void OnClickBoosterMap()
    {
        StartCoroutine(OnClickBoostMapIE());
    }
    public void OnClickBoosterExit()
    {
        StartCoroutine(OnClickBoostExitIE());
    }
    IEnumerator OnClickBoostMapIE()
    {
        float oldpitch = SoundObject.GetComponent<AudioSource>().pitch;
        BountyTextObject.GetComponent<TextMeshProUGUI>().color = new Color32(255, 0, 0, 255);
        for (int i = 0; i < 16; i++)
        {
            if (!CellsInBodyGridObject[i].GetComponent<Animator>().enabled) CellsInBodyGridObject[i].GetComponent<Animator>().enabled = true;
            CellsInBodyGridObject[i].GetComponent<Animator>().SetTrigger("Start");
            SoundObject.GetComponent<AudioSource>().PlayOneShot(ShelkSound);
            SoundObject.GetComponent<AudioSource>().pitch = SoundObject.GetComponent<AudioSource>().pitch + 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
        SoundObject.GetComponent<AudioSource>().pitch = oldpitch;
        TotalScoreUpdate(0, -PriceToOpenMap);
        BountyTextObject.GetComponent<TextMeshProUGUI>().text = UsTotalBounty.ToString();
        for (int i = 0; i < 16; i++)
        {
            CellsInBodyGridObject[i].GetComponent<Animator>().SetTrigger("End");
            yield return new WaitForSeconds(0.05f);
            SoundObject.GetComponent<AudioSource>().PlayOneShot(ShelkSound);
            SoundObject.GetComponent<AudioSource>().pitch = SoundObject.GetComponent<AudioSource>().pitch + 0.1f;
        }
        BountyTextObject.GetComponent<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255);
        SoundObject.GetComponent<AudioSource>().pitch = oldpitch;
        if (UsTotalBounty >= PriceToOpenExit) FooterObject.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
        if (UsTotalBounty >= PriceToOpenMap) FooterObject.gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;

    }
    IEnumerator OnClickBoostExitIE()
    {
        float oldpitch = SoundObject.GetComponent<AudioSource>().pitch;
        BountyTextObject.GetComponent<TextMeshProUGUI>().color = new Color32(255, 0, 0, 255);
        for (int i = 0; i < 16; i++)
        {
            if (!CellsInBodyGridObject[i].GetComponent<Animator>().enabled) CellsInBodyGridObject[i].GetComponent<Animator>().enabled = true;
            CellsInBodyGridObject[i].GetComponent<Animator>().SetTrigger("Start");
            SoundObject.GetComponent<AudioSource>().PlayOneShot(ShelkSound);
            SoundObject.GetComponent<AudioSource>().pitch = SoundObject.GetComponent<AudioSource>().pitch + 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
        for (int i = 0; i < 16; i++)
        {
            if (FullMap[i].Mask == -5)
            {
                FullMap[i].Mask = -8;
                MapUpdate(i, false);
                CellsInBodyGridObject[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = RedDoorBtn;
            }
        }
        SoundObject.GetComponent<AudioSource>().pitch = oldpitch;
        TotalScoreUpdate(0, -PriceToOpenExit);
        BountyTextObject.GetComponent<TextMeshProUGUI>().text = UsTotalBounty.ToString();
        for (int i = 0; i < 16; i++)
        {
            CellsInBodyGridObject[i].GetComponent<Animator>().SetTrigger("End");
            yield return new WaitForSeconds(0.05f);
            SoundObject.GetComponent<AudioSource>().PlayOneShot(ShelkSound);
            SoundObject.GetComponent<AudioSource>().pitch = SoundObject.GetComponent<AudioSource>().pitch + 0.1f;
        }
        BountyTextObject.GetComponent<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255);
        SoundObject.GetComponent<AudioSource>().pitch = oldpitch;
        if (UsTotalBounty >= PriceToOpenExit) FooterObject.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
        else FooterObject.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = false;
        if (UsTotalBounty >= PriceToOpenMap) FooterObject.gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
        else FooterObject.gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = false;
    }
    public void OnClickSettingsBtn()
    {
        StartCoroutine(OpenSettings());
    }
    public void OnClickSettingsCloseBtn()
    {
        StartCoroutine(CloseSettings());
    }
    public void HelperAnsBtnOnClick(int whatcadr)
    {
        StartCoroutine(HelperAnsBtnOnClickAnim(whatcadr));
    }
    IEnumerator OpenSettings()
    {
        BokPanelOpened = false;
        SettingsOpened = true;
        BokPanelObject.GetComponent<CanvasGroup>().interactable = false;
        SettingsBoxObject.active = true;
        VolumeSliderObject.GetComponent<Slider>().value = UsVolume;
        VolumeSliderObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().color = new Color32((byte)((1 - UsVolume) * 255), (byte)(UsVolume * 255), 0, 255);
        if (UsVolumeChek) ChekerSoundBtnObject.GetComponent<Image>().sprite = GreenBtn;
        else ChekerSoundBtnObject.GetComponent<Image>().sprite = RedBtn;
        SettingsBoxObject.GetComponent<Animator>().enabled = true;
        SettingsBoxObject.GetComponent<Animator>().SetTrigger("Show");
        yield return new WaitForSeconds(0.30f);
    }
   
    IEnumerator CloseSettings()
    {
        SettingsOpened = false;
        BokPanelOpened = true;
        SettingsBoxObject.GetComponent<Animator>().enabled = true;
        SettingsBoxObject.GetComponent<Animator>().SetTrigger("Hide");
        yield return new WaitForSeconds(0.30f);
        SettingsBoxObject.active = false;
        BokPanelObject.GetComponent<CanvasGroup>().interactable = true;
    }
    IEnumerator HelperAnsBtnOnClickAnim(int whatcadr)
    {
        if (whatcadr == -1)
        {
            HelperAnsBtnObjects[1].GetComponent<Button>().onClick.RemoveAllListeners();
            UsHelperActive = 0;
        }
        if (whatcadr == 0)
        {
            HelperAnsBtnObjects[1].GetComponent<Button>().interactable = false;
            HelperAnsBtnObjects[1].GetComponent<Animator>().SetTrigger("Destroy");
            yield return new WaitForSeconds(0.1f);
        }
        if (whatcadr == 4)
        {
            UsHelperActive = 0;
            StartCoroutine(TimerOschet());
        }
        HelperAnsBtnObjects[0].GetComponent<Button>().interactable = false;
        HelperAnsBtnObjects[0].GetComponent<Animator>().SetTrigger("Destroy");
        yield return new WaitForSeconds(0.2f);
        HelperObject.GetComponent<Animator>().SetTrigger("HiseShow");
        yield return new WaitForSeconds(0.35f);
        HelperObject.active = false;
        HelperAnsBtnObjects[0].active = false;
        HelperAnsBtnObjects[1].active = false;
        if (whatcadr < 3 && !(whatcadr == -1)) StartCoroutine(StartHelp(whatcadr + 1));
        if (whatcadr == 3) UsHelperActive = 2;

    }
    public void OnChangeVolumeSlider()
    {
        UsVolume = VolumeSliderObject.GetComponent<Slider>().value;
        BackgroundObject.GetComponent<AudioSource>().volume = UsVolume;
        VolumeSliderObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().color = new Color32((byte)((1 - UsVolume) * 255), (byte)(UsVolume * 255), 0, 255);
    }
    public void OnClickGumburg()
    {
        StartCoroutine(OpenBokPanel());
    }
    public void OnClickCloseBokPanel()
    {
        StartCoroutine(CloseBokPanel());
    }
    public void OnClickAnswerBtn(AButtonLook answer)
    {
        TimerCheker = false;
        Debug.Log(answer.text);
        if (answer.index == -1)
            for (int i = 0; i < 4; i++)
            {
                AnswerButtonObjects[i].GetComponent<Image>().sprite = RedAnswerBtn;
            }
        else AnswerButtonObjects[answer.index].GetComponent<Image>().sprite = RedAnswerBtn;
        AnswerButtonObjects[IndexOfRightAnswer].GetComponent<Image>().sprite = GreenAnswerBtn;
        for (int i=0;i<4;i++)
        {
            AnswerButtonObjects[i].GetComponent<Button>().interactable = false;
        }
        int deltascore = 5;
        int deltabounty = 1;
        if (TimerCounter>0.75f)
        {
            deltascore = (int)(deltascore + (TimerCounter * TimerCounter * 10));
            deltabounty = (int)(deltabounty + (TimerCounter * TimerCounter * 10));
        }

        BackgroundObject.GetComponent<AudioSource>().Stop();
        if (answer.index == IndexOfRightAnswer)
        {
            BackgroundObject.GetComponent<AudioSource>().PlayOneShot(PositiveAnswerSound);
            TotalScoreUpdate(deltascore, deltabounty);
            MapUpdate(NowCell, true);
        }
        else
        {
            BackgroundObject.GetComponent<AudioSource>().PlayOneShot(NegativeAnswerSound);
            MapUpdate(NowCell, false);
        }
        if (!CoroutineIsRunning)
        {
            StartCoroutine(ShowInform());
            CoroutineIsRunning = true;
        }

    }
    public void TotalScoreUpdate(int sc,int bou)
    {
        InfoPerson per1 = new InfoPerson();
        per1.TotalScore = UsTotalScore + sc;
        per1.TotalBounty = UsTotalBounty + bou;
        UsTotalBounty = UsTotalBounty + bou;
        UsTotalScore = UsTotalScore + sc;
        per1.HelperActive = UsHelperActive;
        per1.Volume = UsVolume;
        per1.VolumeChek = UsVolumeChek;

        string per1inf = JsonUtility.ToJson(per1);
        File.WriteAllText(PathToInfo, per1inf);
    }
    public void MapUpdate(int cella, bool nukak)
    {
        Debug.Log("___________________");
        Debug.Log(cella);
        Debug.Log(FullMap[cella].Mask);
        Debug.Log("___________________");
        if (FullMap[cella].Mask == -8)
            if (nukak) FullMap[cella].Mask = -45;
            else FullMap[cella].Mask = -35;
        else
            if (nukak) FullMap[cella].Mask = -4;
            else FullMap[cella].Mask = -3;
        if ((cella % 4 == 1) || (cella % 4 == 2))
        {
            if (FullMap[cella - 1].Mask == -1) FullMap[cella - 1].Mask = -2;
            if (FullMap[cella + 1].Mask == -1) FullMap[cella + 1].Mask = -2;
            if (FullMap[cella + 1].Mask == -5) FullMap[cella + 1].Mask = -8;
            if (FullMap[cella - 1].Mask == -5) FullMap[cella - 1].Mask = -8;

        }
        else
        {
            if (cella % 4 == 0) if (FullMap[cella + 1].Mask == -5) FullMap[cella + 1].Mask = -8;
            if (cella % 4 == 3) if (FullMap[cella - 1].Mask == -5) FullMap[cella - 1].Mask = -8;
            if (cella % 4 == 0) if (FullMap[cella + 1].Mask == -1) FullMap[cella + 1].Mask = -2;
            if (cella % 4 == 3) if (FullMap[cella - 1].Mask == -1) FullMap[cella - 1].Mask = -2;

        }
        if (cella - 4 >= 0)
            if (FullMap[cella - 4].Mask == -1) FullMap[cella - 4].Mask = -2;
        if (cella + 4 < FullMap.Length)
            if (FullMap[cella + 4].Mask == -1) FullMap[cella + 4].Mask = -2;
        if (cella - 4 >= 0)
            if (FullMap[cella - 4].Mask == -5) FullMap[cella - 4].Mask = -8;
        if (cella + 4 < FullMap.Length)
            if (FullMap[cella + 4].Mask == -5) FullMap[cella + 4].Mask = -8;
        int[] newmap = new int[16];
        int[] newmask = new int[16];
        for (int i =0; i<16;i++)
        {
            newmap[i] = FullMap[i].Map;
            newmask[i] = FullMap[i].Mask;
        }
        WritetoCSVMap(PathToSCVMap, newmap);
        WritetoCSVMap(PathToSCVMSK, newmask);
    }

    public void OnClickNextBtn()
    { 
        StartCoroutine(AfterNextBtn());
    }
    IEnumerator AfterNextBtn()
    {
        if (!CellsInBodyGridObject[NowCell].GetComponent<Animator>().enabled) CellsInBodyGridObject[NowCell].GetComponent<Animator>().enabled = true;
        CellsInBodyGridObject[NowCell].GetComponent<Animator>().SetTrigger("Closed");
        if (!QNALayoutObject.gameObject.GetComponent<Animator>().enabled) QNALayoutObject.gameObject.GetComponent<Animator>().enabled = true;
        else
        {
            QNALayoutObject.GetComponent<Animator>().SetTrigger("GoOut");
            NextBtn.GetComponent<Button>().interactable = false;
        }
        yield return new WaitForSeconds(0.25f);
        InformOpened = false;
        StartCoroutine(InitMap());

    }
    IEnumerator ShowInform()
    {
        InformOpened = true;
        int AnimTime = 10;
        QNALayoutObject.GetComponent<Animator>().SetTrigger("ItemOn");
        int lenr = QuestionTextObject.GetComponent<TextMeshProUGUI>().text.Length;
  
        for (int i = 0; i < AnimTime; i++)
        {
            Vector3 ChangeScale = new Vector3(1.0f, (AnimTime -i-1) * 1.0f / AnimTime, 1.0f);
            QuestionTextBoxObject.transform.localScale = ChangeScale;
            yield return new WaitForSeconds(0.01f);
        }
        QuestionTextObject.GetComponent<TextMeshProUGUI>().text = FullList[NowQNum].Clarification;
        NextBtn.GetComponent<Button>().interactable = true;
        for (int i = 0; i < AnimTime; i++)
        {
            Vector3 ChangeScale = new Vector3(1.0f, (i + 1) * 1.0f / AnimTime, 1.0f);
            QuestionTextBoxObject.transform.localScale = ChangeScale;
            QuestionTextBoxObject.gameObject.GetComponent<VerticalLayoutGroup>().spacing += 1;
            QuestionTextBoxObject.gameObject.GetComponent<VerticalLayoutGroup>().spacing -= 1;
            yield return new WaitForSeconds(0.01f);
        }
        CoroutineIsRunning = false;
    }
    IEnumerator OpenCellNQNA(int q,int Num)
    {
        MainMenuOpened = false;
        CellOpened = true;
        RectTransform rt1 = QNALayoutObject.GetComponent<RectTransform>();
        rt1.sizeDelta = new Vector2(1080, CanvasObject.GetComponent<RectTransform>().rect.height);
        BackgroundObject.GetComponent<AudioSource>().Stop();
        BackgroundObject.GetComponent<AudioSource>().PlayOneShot(OpenCellSound);

        NowCell = q;
        NowQNum = Num-1;
        Debug.Log("Now try to open " + NowCell.ToString() + " cell, ques = " + NowQNum.ToString());
        QNALayoutObject.active = true;
        int uo = FullList[Num - 1].NameOfImage.Length - 4;
        Debug.Log("Try to load  " + FullList[Num - 1].NameOfImage.Remove(FullList[Num - 1].NameOfImage.Length - 4));
        for (int i = 0; i < 4; i++) AnswerButtonObjects[i].GetComponent<Image>().sprite = NormalAnswerBtn;
        if (!CellsInBodyGridObject[NowCell].GetComponent<Animator>().enabled) CellsInBodyGridObject[NowCell].GetComponent<Animator>().enabled = true;
        CellsInBodyGridObject[NowCell].GetComponent<Animator>().SetTrigger("Open");
        yield return new WaitForSeconds(0.1f);
        Debug.Log("qestion value = " + FullList[Num - 1].Question);
        QuestionTextObject.GetComponent<TextMeshProUGUI>().text = FullList[Num - 1].Question;
        //Texture2D QImage = LoadPNG(Application.persistentDataPath + "/" + FullList[Num].NameOfImage);

        Texture2D QImage = Resources.Load<Texture2D>("Images/" + FullList[Num - 1].NameOfImage.Remove(FullList[Num - 1].NameOfImage.Length - 4));
        RectTransform rt = (RectTransform)QuestionImageObject.gameObject.transform;
        float scalee = QImage.width / rt.rect.width;
        rt.sizeDelta = new Vector2(rt.rect.width, QImage.height / scalee);
        /*QuestionImageObject.GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/"+FullList[Num].NameOfImage.Remove(FullList[Num].NameOfImage.Length-4));*/
        //QuestionImageObject.GetComponent<RawImage>().texture = LoadPNG(Application.persistentDataPath + "/" + FullList[Num].NameOfImage);     OLD LOAD IMAGE
        QuestionImageObject.GetComponent<RawImage>().texture = QImage;
        QuestionImageBoxObject.gameObject.GetComponent<ContentSizeFitter>().enabled = false;
        QuestionImageBoxObject.gameObject.GetComponent<ContentSizeFitter>().enabled = true;
        List<string> answers = new List<string>(FullList[Num - 1].Answers);
        for (int i = 0; i < 4; i++)
        {
            int rand = UnityEngine.Random.Range(0, answers.Count);
            if (answers[rand] == FullList[Num - 1].RightA) IndexOfRightAnswer = i;
            AnswerButtonObjects[i].gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = answers[rand];
            AButtonLook temporlook = new AButtonLook();
            temporlook.index = i;
            temporlook.text = answers[rand];
            AnswerButtonObjects[i].gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            AnswerButtonObjects[i].gameObject.GetComponent<Button>().onClick.AddListener(delegate { OnClickAnswerBtn(temporlook); });
            answers.RemoveAt(rand);
        }
        
        if (!UpperObject.gameObject.GetComponent<Animator>().enabled) UpperObject.gameObject.GetComponent<Animator>().enabled = true;
        UpperObject.gameObject.GetComponent<Animator>().SetTrigger("Off");
        if (!BodyGridBoxObject.gameObject.GetComponent<Animator>().enabled) BodyGridBoxObject.gameObject.GetComponent<Animator>().enabled = true;
        BodyGridBoxObject.gameObject.GetComponent<Animator>().SetTrigger("OffAnim");
        if (!FooterObject.gameObject.GetComponent<Animator>().enabled) FooterObject.gameObject.GetComponent<Animator>().enabled = true;
        FooterObject.gameObject.GetComponent<Animator>().SetTrigger("Off");
        UpperObject.gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
        
        for (int i = 0; i < 16; i++)
            if (FullMap[i].Map != 0) CellsInBodyGridObject[i].GetComponent<Button>().interactable = false;
        FooterObject.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = false;
        FooterObject.gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = false;
        yield return new WaitForSeconds(0.25f);
        
        if (!QNALayoutObject.gameObject.GetComponent<Animator>().enabled) QNALayoutObject.gameObject.GetComponent<Animator>().enabled = true;
        else
        {
            //QNALayoutObject.gameObject.GetComponent<Animator>().SetTrigger("GoIn");
            QNALayoutObject.GetComponent<Animator>().SetTrigger("ItemsOn");
            NextBtn.GetComponent<Button>().interactable = false;
        }
        StartCoroutine(SliderGrow());
        yield return new WaitForSeconds(0.27f);
        if (UsHelperActive == 2) StartCoroutine(StartHelp(4));
        else StartCoroutine(TimerOschet());
        QNABodyBoxObject.gameObject.GetComponent<VerticalLayoutGroup>().spacing += 1;
        yield return new WaitForSeconds(0.01f);
        QNABodyBoxObject.gameObject.GetComponent<VerticalLayoutGroup>().spacing -= 1;
        yield return new WaitForSeconds(0.01f);
        float BodySize = QNABodyBoxObject.GetComponent<RectTransform>().rect.height;
        Debug.Log(BodySize);
        QNABodyBoxObject.transform.localPosition = new Vector3(0, rt1.rect.height / 2 - BodySize / 2 - 60, 0);
        Debug.Log(rt1.rect.height / 2 - BodySize / 2 - 60);

        for (int i = 0; i < 4; i++)
        {
            if (!AnswerButtonObjects[i].gameObject.GetComponent<Animator>().enabled) AnswerButtonObjects[i].gameObject.GetComponent<Animator>().enabled = true;
            else AnswerButtonObjects[i].gameObject.GetComponent<Animator>().SetTrigger("Create");
            yield return new WaitForSeconds(0.1f);

        }
        for (int i = 0; i < 4; i++) AnswerButtonObjects[i].gameObject.GetComponent<Button>().interactable = true;
        Debug.Log(QNABodyBoxObject.GetComponent<RectTransform>().rect.height);
    }

    IEnumerator SliderGrow()
    {
        for (int i =0; i<25;i++)
        {
            SliderTimer.value += 0.04f;
            yield return new WaitForSeconds(0.01f);
        }
    }
    IEnumerator TimerOschet()
    {
        Debug.Log("Timerstart");
        BackgroundObject.GetComponent<AudioSource>().enabled = true;
        BackgroundObject.GetComponent<AudioSource>().loop = true;
        BackgroundObject.GetComponent<AudioSource>().clip = TicTacSound;
        BackgroundObject.GetComponent<AudioSource>().Play();
        TimerCheker = true;
        float timee = 1.0f;
        while(TimerCheker)
        {
            timee -= 0.01f;
            TimerCounter = timee;
            byte rc = (byte)((1 - timee) * 255); 
            byte gc = (byte)(timee * 255);
            SliderTimer.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().color = new Color32(rc, gc, 0, 255);
            SliderTimer.value = timee;
            yield return new WaitForSeconds(0.1f);
            if (timee <= 0)
            {

                TimerCheker = false;
                AButtonLook timeout = new AButtonLook();
                timeout.index = -1;
                timeout.text = "timeout";
                OnClickAnswerBtn(timeout);
                break;
            }
        }
        
    }
    IEnumerator CloseBokPanel()
    {
        for (int i = 0; i < 3; i++) BokPanelObject.gameObject.transform.GetChild(1).gameObject.transform.GetChild(i).gameObject.GetComponent<Button>().interactable = false;
        BokPanelObject.gameObject.GetComponent<Animator>().SetTrigger("Off");
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < 16; i++)
            if (FullMap[i].Map != 0) CellsInBodyGridObject[i].GetComponent<Button>().interactable = true;
        if (UsTotalBounty >= PriceToOpenExit) FooterObject.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
        if (UsTotalBounty >= PriceToOpenMap) FooterObject.gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
        BokPanelObject.active = false;
        MainMenuOpened = true;
        BokPanelOpened = false;

    }
    IEnumerator OpenBokPanel()
    {
        MainMenuOpened = false;
        BokPanelOpened = true;
        BokPanelObject.active = true;
        for (int i = 0; i < 16; i++) CellsInBodyGridObject[i].GetComponent<Button>().interactable = false;
        FooterObject.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = false;
        FooterObject.gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.GetComponent<Button>().interactable = false;
        if (!BokPanelObject.gameObject.GetComponent<Animator>().enabled) BokPanelObject.gameObject.GetComponent<Animator>().enabled = true;
        else BokPanelObject.gameObject.GetComponent<Animator>().SetTrigger("On");
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < 3; i++) BokPanelObject.gameObject.transform.GetChild(1).gameObject.transform.GetChild(i).gameObject.GetComponent<Button>().interactable = true;
    }

    public QandA[] ReadCSVFile(string path)
    {
        string[] lines;
        string[] linesdat;
        string fileData = System.IO.File.ReadAllText(path);
        lines = fileData.Split("\n"[0]);
        int quantque = lines.Length - 1;
        for (int i = 0; i < quantque; i++)
        {
            Debug.Log(lines[i]);
            Debug.Log("__________________________");
        }
        QandA[] Full = new QandA[quantque];
        for (int i = 0; i < quantque; i++)
        {
            Debug.Log(lines[i]);
            linesdat = (lines[i].Trim()).Split(";"[0]);
            Full[i].Question = linesdat[0];
            Full[i].RightA = linesdat[1];
            Full[i].Answers = new string[4];
            for (int k = 0; k < 4; k++)
                Full[i].Answers[k] = linesdat[k + 1];
            Full[i].NameOfImage = linesdat[5];
        }
        return Full;
    }
    public void WritetoCSVMap(string path, int[] mass)
    {
        var writer = File.CreateText(path);
        /*StreamWriter writer = new StreamWriter(path, true);*/
        for (int i = 0; i < 4; i++) 
        {
            string line = string.Empty;
            for (int j = 0; j < 4; j++) 
            {
                line = line + mass[i * 4 + j].ToString();
                if (j != 3) line = line + ";";
            }
            writer.WriteLine(line);
        }
        writer.Close();
    }

    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        return tex;
    }
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android) {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (SettingsOpened)
                {
                    StartCoroutine(CloseSettings());
                    return;
                }
                if (BokPanelOpened)
                {
                    StartCoroutine(CloseBokPanel());
                    return;
                }
                if (InformOpened)
                {
                    StartCoroutine(AfterNextBtn());
                    return;
                }
                if (MainMenuOpened)
                {
                    Application.Quit();
                }
            }
        }
    }
}
public struct ListContainer
{
    public List<QuestionDataModel> dataList;
    public ListContainer(List<QuestionDataModel> _dataList)
    {
        dataList = _dataList;
    }
}
public struct InfoPerson
{
    public int TotalScore;
    public int TotalBounty;
    public int HelperActive;
    public float Volume;
    public bool VolumeChek;

    public InfoPerson(int _TotalScore, int _TotalBounty, int _HelperActive, float _Volume, bool _VolumeChek)
    {
        TotalScore = _TotalScore;
        TotalBounty = _TotalBounty;
        HelperActive = _HelperActive;
        Volume = _Volume;
        VolumeChek = _VolumeChek;
    }
}

[Serializable]
public struct QuestionDataModel
{
    public string QuestionTS;
    public string RightAnswer;
    public string[] Answers;
    public string Clarification;
    public string PathToImage;
  
    public QuestionDataModel(string _QuestionTS, string _RightAnswer, string[] _Answers, string _Clarification, string _PathToImage)
    {
        QuestionTS = _QuestionTS;
        RightAnswer = _RightAnswer;
        Answers = _Answers;
        Clarification = _Clarification;
        PathToImage = _PathToImage;
    }
}