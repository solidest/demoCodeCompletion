using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace demoCodeCompletion
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CodeEditor.TextArea.TextEntering += CodeEditor_TextArea_TextEntering;
            CodeEditor.TextArea.TextEntered += CodeEditor_TextArea_TextEntered;
            InitialTips();
        }

        private CompletionWindow _completionWindow;
        private Dictionary<string, IList<MyCompletionData>> _tips;

        private void CodeEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (_completionWindow != null) return;
            IList<MyCompletionData> tipData = null;
            var lead = GetLeading();
            bool hasDot = false;
            if (lead.Contains("."))
            {
                hasDot = true;
                lead = lead.Split('.').Last();
            }

            if(lead.Length>0)
            {
                OutText.AppendText("请求提示：" + lead + Environment.NewLine);
                OutText.ScrollToEnd();
            }

            if (e.Text == ".")
            {
                if (0 == _tips["animals"].Where(p => p.Text == lead).Count())
                    return;
                tipData = _tips["actions"];
            }
            else if(e.Text == "(")  
            {

                //TODO 函数提示
                return;
            }
            else if(char.IsLetterOrDigit(e.Text[0]) && !hasDot)
            {
                if(0 == _tips["animals"].Where(p => p.Text.StartsWith(lead, StringComparison.CurrentCultureIgnoreCase)).Count())
                    return;
                tipData = _tips["animals"];
                
            }

            if (tipData == null) return;
            _completionWindow = new CompletionWindow(CodeEditor.TextArea);
            var data = _completionWindow.CompletionList.CompletionData;
            foreach (var item in tipData)
            {
                data.Add(item);
            }
            _completionWindow.Show();
            _completionWindow.Closed += delegate { _completionWindow = null; };
    
        }

        private void CodeEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private string GetLeading()
        {
            var line = CodeEditor.Document.GetLineByOffset(CodeEditor.CaretOffset);
            var lead = CodeEditor.Document.GetText(line.Offset, CodeEditor.CaretOffset - line.Offset).ToArray();
            if (lead.Length == 0) return "";
            if (lead.Length == 1) return char.IsLetterOrDigit(lead[0]) ? new string(lead) : "";

            var endPos = lead.Length - 1;

            while(endPos>0)
            {
                if(char.IsLetterOrDigit(lead[endPos]) || (lead[endPos] >= 0x4e00 && lead[endPos] <= 0x9fbb))
                {
                    break;
                }
                else
                {
                    endPos--;
                }
            }

            var beginPos = endPos;
            
            while(beginPos>0)
            {
                if ((char.IsLetterOrDigit(lead[beginPos-1]) || lead[beginPos-1] == '.' || (lead[beginPos-1] >= 0x4e00 && lead[beginPos-1] <= 0x9fbb)))
                {
                    beginPos--;
                }
                else
                    break;
            }
            return new string(lead, beginPos, endPos-beginPos+1);
        }

        private void InitialTips()
        {
            _tips = new Dictionary<string, IList<MyCompletionData>>();

            var animals = new List<MyCompletionData>();
            animals.Add(new MyCompletionData("cat"));
            animals.Add(new MyCompletionData("dog"));
            animals.Add(new MyCompletionData("elephant"));
            animals.Add(new MyCompletionData("panda"));
            animals.Add(new MyCompletionData("lion"));
            animals.Add(new MyCompletionData("fox"));
            animals.Add(new MyCompletionData("spider"));
            animals.Add(new MyCompletionData("duck"));
            animals.Add(new MyCompletionData("通道"));
            _tips.Add("animals", animals);

            var actions = new List<MyCompletionData>();
            actions.Add(new MyCompletionData("Eat"));
            actions.Add(new MyCompletionData("Feed"));
            actions.Add(new MyCompletionData("Run"));
            actions.Add(new MyCompletionData("PlayGame"));
            _tips.Add("actions", actions);
        }
    }
}

public class MyCompletionData : ICompletionData
{
    public MyCompletionData(string text)
    {
        this.Text = text;
    }

    public System.Windows.Media.ImageSource Image
    {
        get { return null; }
    }

    public string Text { get; private set; }

    // Use this property if you want to show a fancy UIElement in the list.
    public object Content
    {
        get { return this.Text; }
    }

    public object Description
    {
        get { return "Description for " + this.Text; }
    }

    public double Priority => 1;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        var line = textArea.Document.GetLineByOffset(completionSegment.Offset);
        var lead = textArea.Document.GetText(line).ToArray();

        int iBegin = completionSegment.Offset - line.Offset;
        int iEnd = completionSegment.EndOffset - line.Offset;

        while (iBegin>0)
        {
            if ((char.IsLetterOrDigit(lead[iBegin - 1]) || (lead[iBegin - 1] >= 0x4e00 && lead[iBegin - 1] <= 0x9fbb)))
            {
                iBegin--;
            }
            else
                break;
        }

        while(iEnd<lead.Length)
        {
            if ((char.IsLetterOrDigit(lead[iEnd]) || (lead[iEnd] >= 0x4e00 && lead[iEnd] <= 0x9fbb)))
            {
                iEnd++;
            }
            else
            {
                break;

            }
        }

        var seg = new TextSegment();
        seg.StartOffset = line.Offset + iBegin;
        seg.EndOffset = line.Offset + iEnd;
        textArea.Document.Replace(seg, this.Text);
    }
}
