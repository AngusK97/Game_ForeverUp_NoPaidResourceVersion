using System.Collections.Generic;

namespace Data
{
    public class StoryData
    {
        public string Title;
        public string Description;
        public List<string> SuccessComments;
        public List<string> FailComments;
        public List<string> SpecialComments;
    }
    
    public partial class DataManager
    {
        //-------------------------------------------------------------------------------------
        // Story Content
        //-------------------------------------------------------------------------------------

        public Dictionary<PhaseType, StoryData> StoryDataDict = new()
        {
            {
                PhaseType.Phase1, new StoryData
                {
                    Title = "阶段1：学生时期",
                    Description = "“好好读书，才能让家长放心，才能考上好大学找到好工作。”",
                    SuccessComments = new List<string>
                    {
                        "“表现得不错，这次终于考了全班第一！不过，99分还是少了一分，下次要再加把劲。”",
                        "“成绩还行，但细节上还有提升空间。小李能考100分，你为什么不能？”",
                        "“这次不错啊！不过隔壁家孩子已经保送了重点高中，加油！”"
                    },
                    FailComments = new List<string>
                    {
                        "“你看看这次才考了80分！是不是最近偷懒了？”",
                        "“就这成绩？年级排名掉到50开外了吧？”",
                        "“别人家的孩子都年级前三，你这样怎么对得起家里人？”"
                    },
                    SpecialComments = new List<string>
                    {
                        "“别人都在补课，而你就知道玩游戏，爸妈的脸以后没地方搁！考不上大学看你怎么办！”",
                        "“玩物丧志，你这样只会给家里丢脸，别人家孩子怎么不学你？”",
                        "“难怪成绩退步，原来心思都在这些没用的东西上！”"
                    }
                }
            },
            {
                PhaseType.Phase2, new StoryData
                {
                    Title = "阶段2：职场初期",
                    Description = "“成家立业是人生大事，必须买房买车，孩子要上好学校。”",
                    SuccessComments = new List<string>
                    {
                        "“表现不错，这次加班抢单成功！不过还是得注意效率。”",
                        "“你加班很厉害，但我听说小王已经被提拔了。”",
                        "“不错，奖金多拿了点。但你什么时候能攒够首付？”"
                    },
                    FailComments = new List<string>
                    {
                        "“这么重要的客户都搞不定，你对得起公司吗？”",
                        "“大家都在拼命，你这样懒散能有前途？”",
                        "“小王买房结婚了，你还在原地踏步。”"
                    },
                    SpecialComments = new List<string>
                    {
                        "“工作不努力，天天搞这些没用的东西，公司迟早会把你裁掉。”",
                        "“你以为自己是谁？艺术家？不踏实工作怎么立足？”",
                        "“你不加班就算了，还搞这些乱七八糟的东西，升职加薪别想了！”"
                    }
                }
            },
            {
                PhaseType.Phase3, new StoryData
                {
                    Title = "阶段3：中年时期",
                    Description = "“中年危机？没有危机感才是真的危机！房贷、车贷、孩子教育、赡养老人，每一个都不能出问题。”",
                    SuccessComments = new List<string>
                    {
                        "“不错，房贷终于还清了。但接下来孩子培训班费用怎么办？”",
                        "“听说你最近换了个好车，挺有面子的，但压力大吧？”",
                        "“父母年纪大了，你可得多回家看看，别光顾着工作。”"
                    },
                    FailComments = new List<string>
                    {
                        "“房贷还不上了？是不是工作上不努力？”",
                        "“都这个年纪了还在租房，太没安全感了吧？”",
                        "“你怎么老说忙，都不回来看看我们？”"
                    },
                    SpecialComments = new List<string>
                    {
                        "“都这把年纪了，还想着看这些没用的书？怎么不多存点钱留给孩子？”",
                        "“净整这些虚头巴脑的东西，万一以后养老靠不上，谁管你？”",
                        "“你这样只顾自己，难道不怕孩子以后对你失望？”"
                    }
                }
            },
            {
                PhaseType.Phase4, new StoryData
                {
                    Title = "阶段4：终极追逐",
                    Description = "你终于看到了生命的意义，但...它好像比你想象得更远、更难触及。",
                    FailComments = new List<string>
                    {
                        "我这一生，究竟有没有真的活过？",
                        "......",
                        "“他是个令人尊敬的人，一生兢兢业业，为家庭、公司奉献了一切。”",
                        "“他是一个成功的典范，他的一生值得被铭记。”",
                        "......",
                        "孩子：“虽然他离开了，但我一定要继续像他一样优秀！”",
                    }
                }
            },
        };
    }
}