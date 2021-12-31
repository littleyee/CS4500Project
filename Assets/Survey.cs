using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class Survey : MonoBehaviour
{
    public TextMeshPro heading;
    public TextMeshPro text;
    public TextMeshPro enter;
    public CoRunner givenCoRunner;

    public static CoRunner runner;

    // settable within Unity editor
    public List<Item> Items;

    private Trial[] trials;
    private Trial currentTrial;
    private int trialIndex = 0;

    // the mutable list of fetched and fetchable items
    private List<Item> fetchable;
    private List<Item> fetched;

    // Start is called before the first frame update
    void Start()
    {
        runner = givenCoRunner;
        fetchable = new List<Item>(Items);
        fetched = new List<Item>();
        SetUpTrials();
        currentTrial = trials[trialIndex];
        currentTrial.Begin();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // sends digit to current Trial for user sequence
    public void SendDigit(string digit) {
        currentTrial.SendDigit(digit);
    }

    // goes to the next page in the survey
    public void NextPage() {
        Debug.Log("Current Trial: " + trialIndex);
        // end early if index out of bounds
        if (trialIndex >= trials.Length) return; 
        // if we can't get the next page within a trial,
        // then we need to go forward to the next trial
        if (!currentTrial.TrialNextPage()) {
            trialIndex++;
            if (trialIndex == trials.Length - 1) ExportCSV();

            Debug.Log("Going to Trial: " + trialIndex);
            // make sure we haven't gone out of bounds
            if (trialIndex >= trials.Length) return;

            Debug.Log("Showing next trial");
            // move to and begin next trial
            currentTrial = trials[trialIndex];
            currentTrial.Begin();
        }
    }

    private void ExportCSV() {
        string filePath = Path.Combine(Application.persistentDataPath, string.Format("vr-working-memory_{0}.csv", System.DateTime.Now.ToString("MM-dd-yy_h-mm-tt")));
        CSVWriter writer = new CSVWriter(filePath);

        writer.WriteRow(new string[] {"Trial", "Correct Sequence", "User Sequence", "Correct",
                        "Order", "Task Action", "Task Item"});

        for (int i = 1; i <= 12; i++) {
            Trial t = trials[i];
            string sequence = t.GetSequence();
            string userSequence = t.GetUserSequence();
            writer.WriteRow(new string[] {t.TrialNumber.ToString(), sequence, userSequence,
                            string.Equals(sequence, userSequence).ToString(), 
                            t.IsReverseSequence ? "reverse" : "forward", t.Task, t.FetchItem});
        }

        writer.Close();
    }

    private class Trial {
        // accessed by CSV creator
        public int TrialNumber;
        public int SequenceLength;
        public bool IsReverseSequence;
        public string Task;
        public string FetchItem;

        // store created sequence and received from user
        private string Sequence = "";
        private string UserSequence = "";

        // the internal portions of of the trial
        private Page[] pages;
        private Page currentPage;
        private int pageIndex = 0;

        public Trial(int number, int sequenceLength, bool isReverse, 
                    string task, string item, Page[] trialPages) {
            TrialNumber = number;
            SequenceLength = sequenceLength;
            IsReverseSequence = isReverse;
            Task = task;
            FetchItem = item;
            pages = trialPages;

            currentPage = pages[pageIndex];
        }

        public string GetSequence() {
            return Sequence;
        }

        public string GetUserSequence() {
            return UserSequence;
        }

        // starts trial by showing first page
        public void Begin() {
            currentPage.Show();
        }

        public bool TrialNextPage() {
            Debug.Log("Current Page: " + pageIndex);
            ExtractCurrentPageData();
            // if the current page can't move forward, survey
            // will interpret Trial as successfully handled
            if (!currentPage.CanMoveForward()) return true;
            pageIndex++;
            Debug.Log("Current Page: " + pageIndex);
            // if we have run out of pages
            if (pageIndex >= pages.Length) return false;

            Debug.Log("Showing next page");
            currentPage = pages[pageIndex];
            currentPage.Show();
            return true;
        }

        // gets the relevant data out of a page for the trial, like the
        // generated sequence or user's entered sequence
        private void ExtractCurrentPageData() {
            if (currentPage is Sequence) {
                Sequence = currentPage.GetSequence();
            }
            else if (currentPage is SequenceEnter) {
                UserSequence = currentPage.GetEnteredSequence();
            }
        }

        public void SendDigit(string digit) {
            currentPage.SendDigit(digit);
        }
    }

    private partial class Page {
        public string Heading;
        public string Text;

        protected TextMeshPro SurveyHeading;
        protected TextMeshPro SurveyText;
        protected TextMeshPro SurveyEnter;

        protected float defaultFontSize = 0.4f;

        protected bool canMoveForward = true;

        private const float SEQUENCE_WAIT = 1f;
        protected WaitForSecondsRealtime DELAY = new WaitForSecondsRealtime(SEQUENCE_WAIT);

        public Page(string heading, string text, TextMeshPro surveyHeading, TextMeshPro surveyText, TextMeshPro surveyEnter) {
            Heading = heading;
            Text = text;
            SurveyHeading = surveyHeading;
            SurveyText = surveyText;
            SurveyEnter = surveyEnter;
        }

        public bool CanMoveForward() {
            return canMoveForward;
        }

        // handles showing the page to the survey screen
        public virtual void Show() {
            SurveyHeading.text = Heading;
            SurveyText.text = Text;
            SurveyEnter.text = "";
        }

        public virtual void SendDigit(string Digit) { }
        public virtual string GetSequence() { return "ERR"; }
        public virtual string GetEnteredSequence() { return "ERR";  }
    }

    private class Sequence : Page {
        private int length;
        private string sequence;
        private bool reverse;

        public Sequence(TextMeshPro surveyHeading, TextMeshPro surveyText, TextMeshPro surveyEnter,
                int length, bool reverse) 
                : base ("", "", surveyHeading, surveyText, surveyEnter) {
            this.length = length;
            this.reverse = reverse;
            sequence = "";
            canMoveForward = false;
        }

        public override string GetSequence() {
            return sequence;
        }

        public override void Show() {
            SurveyHeading.text = "";
            SurveyText.text = "";
            SurveyEnter.text = "";

            runner.StartCoroutine(ShowSequence());
        }

        // coroutine to show sequence of numbers
        IEnumerator ShowSequence() {
            // center text and make it bold with increased size
            SurveyText.alignment = TextAlignmentOptions.MidlineGeoAligned;
            SurveyText.fontSize = 5;
            SurveyText.fontStyle = FontStyles.Bold;

            // wait initially
            yield return DELAY;

            for (int i = 0; i < length; i++) {
                // create digit randomly
                int digit = Random.Range(0, 10);

                // add digit to sequence (prepend for reversed)
                if (reverse) sequence = digit.ToString() + sequence;
                else sequence += digit.ToString();

                SurveyText.text = digit.ToString();
                yield return DELAY;

                SurveyText.text = "";
                yield return DELAY;
            }

            // reset text style
            SurveyText.alignment = TextAlignmentOptions.TopLeft;
            SurveyText.fontSize = defaultFontSize;
            SurveyText.fontStyle = FontStyles.Normal;

            canMoveForward = true;
            SurveyText.text = "Please press the A or X button to continue.";
        }
    }

    private class SequenceEnter : Page {
        private string enteredSequence;
        private int length;
        private bool reverse;

        public SequenceEnter(TextMeshPro surveyHeading, TextMeshPro surveyText, TextMeshPro surveyEnter,
                int length, bool reverse) 
                : base ("", "", surveyHeading, surveyText, surveyEnter) {
            this.length = length;
            this.reverse = reverse;
            enteredSequence = "";
            canMoveForward = false;
        }

        public override string GetEnteredSequence() {
            return enteredSequence;
        }

        public override void Show() {
            SurveyHeading.text = "Recall Sequence";
            if (reverse) SurveyText.text = "Please enter the sequence you memorized earlier IN REVERSE ORDER using the buttons below, then press the A or X button to continue.";
            else SurveyText.text = "Please enter the sequence you memorized earlier using the buttons below, then press the A or X button to continue.";
            SurveyEnter.text = "";
            ShowSequence();
        }

        private void ShowSequence() {
            string[] places = new string[length];
            for (int i = 0; i < length; i++) { 
                places[i] = "_";
                if (enteredSequence != "" && i < enteredSequence.Length) {
                    places[i] = enteredSequence[i].ToString();
                }
            }
            SurveyEnter.text = string.Join(" ", places);
        }

        public override void SendDigit(string digit) {
            if (digit == "Backspace") {
                if (enteredSequence.Length > 0) {
                    // remove last entered digit, then set not able to continue
                    enteredSequence = enteredSequence.Substring(0, enteredSequence.Length - 1);
                    canMoveForward = false;
                    ShowSequence();
                }
                return;
            }
            // don't allow entering longer sequences
            if (enteredSequence.Length >= length) return;

            enteredSequence += digit;
            if (enteredSequence.Length == length) canMoveForward = true;

            ShowSequence();
        }
    }

    // used to instantiate trials
    private void SetUpTrials() {
        trials = new Trial[14];
        (string action, string item, string title, string content) task;
        // intro
        trials[0] = new Trial(0, 1, false, "", "", new Page[] {
            new Page("Experiment",
                     "Beginning on the next page, you will be asked to remember a sequence of numbers. The numbers will be shown individually, and it is your job to remember those numbers, beginning with only three digits and going up to eight digits.. There will be twelve trials total. Between memorizing each sequence and entering it, you will be asked to either get an item from somewhere in the environment or put it back. You will use the keypad below to enter the sequence by touching the green rectangles coming off of your hands to the buttons. Please press the A or X button to continue.",
                     heading, text, enter)
        });
        // Trial 1
        task = CreateTask();
        trials[1] = new Trial(1, 3, false, task.action, task.item, new Page[] {
            new Page("Trial 1", 
                     "On the next page you will be shown three numbers. Please do your best to remember them. When you are ready, press the A or X button to begin showing the sequence.", 
                     heading, text, enter),
            new Sequence(heading, text, enter, 3, false),
            new Page(task.title, task.content, heading, text, enter),
            new SequenceEnter(heading, text, enter, 3, false)
        });
        // Trial 2
        task = CreateTask();
        trials[2] = new Trial(2, 4, false, task.action, task.item, new Page[] {
            new Page("Trial 2", 
                     "On the next page you will be shown four numbers. Please do your best to remember them. When you are ready, press the A or X button to begin showing the sequence.", 
                     heading, text, enter),
            new Sequence(heading, text, enter, 4, false),
            new Page(task.title, task.content, heading, text, enter),
            new SequenceEnter(heading, text, enter, 4, false)
        });
        // Trial 3
        task = CreateTask();
        trials[3] = new Trial(3, 5, false, task.action, task.item, new Page[] {
            new Page("Trial 3", 
                     "On the next page you will be shown five numbers. Please do your best to remember them. When you are ready, press the A or X button to begin showing the sequence.", 
                     heading, text, enter),
            new Sequence(heading, text, enter, 5, false),
            new Page(task.title, task.content, heading, text, enter),
            new SequenceEnter(heading, text, enter, 5, false)
        });
        // Trial 4
        task = CreateTask();
        trials[4] = new Trial(4, 6, false, task.action, task.item, new Page[] {
            new Page("Trial 4", 
                     "On the next page you will be shown six numbers. Please do your best to remember them. When you are ready, press the A or X button to begin showing the sequence.", 
                     heading, text, enter),
            new Sequence(heading, text, enter, 6, false),
            new Page(task.title, task.content, heading, text, enter),
            new SequenceEnter(heading, text, enter, 6, false)
        });
        // Trial 5
        task = CreateTask();
        trials[5] = new Trial(5, 7, false, task.action, task.item, new Page[] {
            new Page("Trial 5", 
                     "On the next page you will be shown seven numbers. Please do your best to remember them. When you are ready, press the A or X button to begin showing the sequence.", 
                     heading, text, enter),
            new Sequence(heading, text, enter, 7, false),
            new Page(task.title, task.content, heading, text, enter),
            new SequenceEnter(heading, text, enter, 7, false)
        });
        // Trial 6
        task = CreateTask();
        trials[6] = new Trial(6, 8, false, task.action, task.item, new Page[] {
            new Page("Trial 6", 
                     "On the next page you will be shown eight numbers. Please do your best to remember them. When you are ready, press the A or X button to begin showing the sequence.", 
                     heading, text, enter),
            new Sequence(heading, text, enter, 8, false),
            new Page(task.title, task.content, heading, text, enter),
            new SequenceEnter(heading, text, enter, 8, false)
        });
        // Trial 7
        task = CreateTask();
        trials[7] = new Trial(7, 3, true, task.action, task.item, new Page[] {
            new Page("Trial 7",
                     "On the next page you will be shown three numbers. Please do your best to remember them. When you are ready, press the A or X button to begin showing the sequence.",
                     heading, text, enter),
            new Sequence(heading, text, enter, 3, true),
            new Page(task.title, task.content, heading, text, enter),
            new SequenceEnter(heading, text, enter, 3, true)
        });
        // Trial 8
        task = CreateTask();
        trials[8] = new Trial(8, 4, true, task.action, task.item, new Page[] {
            new Page("Trial 8",
                     "On the next page you will be shown four numbers. Please do your best to remember them. When you are ready, press the A or X button to begin showing the sequence.",
                     heading, text, enter),
            new Sequence(heading, text, enter, 4, true),
            new Page(task.title, task.content, heading, text, enter),
            new SequenceEnter(heading, text, enter, 4, true)
        });
        // Trial 9
        task = CreateTask();
        trials[9] = new Trial(9, 5, true, task.action, task.item, new Page[] {
            new Page("Trial 9",
                     "On the next page you will be shown five numbers. Please do your best to remember them. When you are ready, press the A or X button to begin showing the sequence.",
                     heading, text, enter),
            new Sequence(heading, text, enter, 5, true),
            new Page(task.title, task.content, heading, text, enter),
            new SequenceEnter(heading, text, enter, 5, true)
        });
        // Trial 10
        task = CreateTask();
        trials[10] = new Trial(10, 6, true, task.action, task.item, new Page[] {
            new Page("Trial 10",
                     "On the next page you will be shown six numbers. Please do your best to remember them. When you are ready, press the A or X button to begin showing the sequence.",
                     heading, text, enter),
            new Sequence(heading, text, enter, 6, true),
            new Page(task.title, task.content, heading, text, enter),
            new SequenceEnter(heading, text, enter, 6, true)
        });
        // Trial 11
        task = CreateTask();
        trials[11] = new Trial(11, 7, true, task.action, task.item, new Page[] {
            new Page("Trial 11",
                     "On the next page you will be shown seven numbers. Please do your best to remember them. When you are ready, press the A or X button to begin showing the sequence.",
                     heading, text, enter),
            new Sequence(heading, text, enter, 7, true),
            new Page(task.title, task.content, heading, text, enter),
            new SequenceEnter(heading, text, enter, 7, true)
        });
        // Trial 12
        task = CreateTask();
        trials[12] = new Trial(12, 8, true, task.action, task.item, new Page[] {
            new Page("Trial 12",
                     "On the next page you will be shown eight numbers. Please do your best to remember them. When you are ready, press the A or X button to begin showing the sequence.",
                     heading, text, enter),
            new Sequence(heading, text, enter, 8, true),
            new Page(task.title, task.content, heading, text, enter),
            new SequenceEnter(heading, text, enter, 8, true)
        });
        // finish
        trials[13] = new Trial(0, 1, false, "", "", new Page[] {
            new Page("Experiment Complete",
                     "Thank you for participating. Please copy the data from the headset to your computer, then follow the instructions from the website. You can now exit the virtual environment.",
                     heading, text, enter)
        });
    }

    private (string, string, string, string) CreateTask() {
        string task = "ERR";
        string itemName = "ERR";
        string title = "ERR";
        string content = "ERR";

        string[] taskTypes = new string[] { "fetch", "return" };

        // get the task (fetch or return)
        if (fetchable.Count > 0 && fetched.Count > 0) task = taskTypes[Random.Range(0, 2)];
        else if (fetched.Count > 0) task = "return";
        else task = "fetch";

        if (task == "return") {
            title = "Return an Item";
            int itemIndex = Random.Range(0, fetched.Count);
            Item item = fetched[itemIndex];
            itemName = item.Name;
            fetched.RemoveAt(itemIndex);
            fetchable.Add(item);
            content = string.Format("Please return the {0} you fetched earlier to its original location ({1}), then return to the TV. When you have done so, please press the A or X button to continue.", item.Name, item.LocationText);
        }
        // task is fetch
        else {
            title = "Fetch an Item";
            int itemIndex = Random.Range(0, fetchable.Count);
            Item item = fetchable[itemIndex];
            itemName = item.Name;
            fetchable.RemoveAt(itemIndex);
            fetched.Add(item);
            content = string.Format("Please go grab the {0} ({1}) and set it on the table to the right of the TV. When you have done so, please press the A or X button to continue.", item.Name, item.LocationText);
        }

        return (task, itemName, title, content);
    }
}