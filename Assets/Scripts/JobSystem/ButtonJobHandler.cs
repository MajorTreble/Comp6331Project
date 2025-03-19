using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ButtonType {Job, Accept, Finish, Departure, Other};
public class ButtonJobHandler : MonoBehaviour
{
    public int index;
    public Button button;
    public ButtonType buttonType;
    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClick);
    }

    public void SetJobButton(int _index, string _name, JobType _type)
    {
        index = _index;
        string text = "";

        text += _name + "\t\t\t";
        text += _type;

        button.GetComponentInChildren<Text>().text = text;
    }

    private void OnButtonClick()
    {
        switch (buttonType)
        {
            case ButtonType.Job:
                if(JobController.Inst.currJob == null)
                    JobView.Inst.ViewJob(index);
            break;
            case ButtonType.Accept:
                JobView.Inst.AcceptJob();
            break;
            case ButtonType.Finish:
                JobView.Inst.FinishJob();
            break;
            case ButtonType.Departure:
                JobView.Inst.Departure();
            break;           
            
            default:
            break;
        }
        
    }
}
