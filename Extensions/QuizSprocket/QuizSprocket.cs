using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jabbot;
using Jabbot.CommandSprockets;
using Jabbot.Models;
using Jabbot.Sprockets.Core;

namespace QuizSprocket
{
    public class QuizSprocket : CommandSprocket
    {
        private enum QuestionType { Celebrity, GeneralKnowledge } ;
        private readonly string _helpInfo;
        private readonly Random _random;
        private readonly Dictionary<QuestionType, Func<IEnumerable<Tuple<string, string, int>>>> _questions;
        private readonly Dictionary<string, int> _scoreTable;
        private Tuple<string, string, int> _lastAskedQuestion;
        private DateTime _lastQuestionAskedAt;
        public decimal CorrectnessFraction { get; private set; }

        public QuizSprocket()
        {
            _helpInfo = "Hi {0}," + Environment.NewLine + Environment.NewLine
                        + "I accept the following commands:" + Environment.NewLine
                        + "celeb:\t" + "request a new celebrity quiz question" + Environment.NewLine
                        + "gk:\t" + "request a new general knowledge quiz question" + Environment.NewLine
                        + "answer:\t" + "answer the last asked question" + Environment.NewLine
                        + "score:\t" + "see your  the last asked question" + Environment.NewLine
                        + "top10/topten:\t" + "see the top10 of the scoretable" + Environment.NewLine
                        + "info/help:\t" + "this message" + Environment.NewLine;

            _random = new Random();

            CelebrityQuestions = InitializeCelebrityQuestions();
            GeneralKnowledgeQuestions = InitializeGeneralKnowledgeQuestions();

            _questions = new Dictionary<QuestionType, Func<IEnumerable<Tuple<string, string, int>>>>
                             {
                                 {QuestionType.Celebrity, () => CelebrityQuestions},
                                 {QuestionType.GeneralKnowledge, () => GeneralKnowledgeQuestions}
                             };

            _scoreTable = new Dictionary<string, int>();
            CorrectnessFraction = 0.6m;
        }

        public IEnumerable<Tuple<string, string, int>> CelebrityQuestions { get; set; }
        public IEnumerable<Tuple<string, string, int>> GeneralKnowledgeQuestions { get; set; }

        #region Question Initializers
        private static IEnumerable<Tuple<string, string, int>> InitializeCelebrityQuestions()
        {
            yield return new Tuple<string, string, int>("Which famous fantasy movie character is played by Daniel Radcliffe?", "Harry Potter", 10);
            yield return new Tuple<string, string, int>("Which kid's TV character is Miley Cyrus better known as?", "Hannah Montana", 10);
            yield return new Tuple<string, string, int>("In which city was pop-queen Madonna born?", "Bay City, Michigan", 10);
            yield return new Tuple<string, string, int>("To whom is Movie star Tom Cruise currently married to?", "Katie Holme", 10);
            yield return new Tuple<string, string, int>("What is Jay-Z's real name?", "Shawn Corey Carter", 10);
            yield return new Tuple<string, string, int>("What was Michael Jackson's pet monkey called?", "Bubbles", 10);
            yield return new Tuple<string, string, int>("What is the highest ever grossing film?", "Avatar", 10);
            yield return new Tuple<string, string, int>("Complete the movie name: Eternal Sunshine of the Spotless _______?", "Mind", 10);
            yield return new Tuple<string, string, int>("Which actor/actress has won the most Oscars?", "Katherine Hepburn", 10);
            yield return new Tuple<string, string, int>("In what year did Elvis Presley die?", "1977", 10);
            yield return new Tuple<string, string, int>("Academy Award-Winning Actor Jon Voight is the father of which famous Actress?", "Angelina Jolie", 10);
            yield return new Tuple<string, string, int>("What is Nicholas Cage's real name?", "Nicolas Kim Coppola", 10);
            yield return new Tuple<string, string, int>("Who Am I?? Musician/Actor born in 1968 in Philadelphia. Starred in popular teen comedy and has been known to be a bit of a 'Bad Boy' and self-proclaimed 'Legend'.....", "Will Smith", 10);
            yield return new Tuple<string, string, int>("In which Southern US city was Pop Superstar Britney Spears born and raised?", "Kentwood, Louisiana", 10);
            yield return new Tuple<string, string, int>("Which celebrity couple called their baby 'Apple'?", "Gwyneth Paltrow and Chris Martin", 10);
            yield return new Tuple<string, string, int>("Complete this movie title - 'While you were... ____', ", "Sleeping", 10);
            yield return new Tuple<string, string, int>("The Indian movie industry is better known as?", "Bollywood", 10);
        }

        private static IEnumerable<Tuple<string, string, int>> InitializeGeneralKnowledgeQuestions()
        {
            yield return new Tuple<string, string, int>("The first President of Bangladesh was", "Sheikh Mujibur Rahman", 10);
            yield return new Tuple<string, string, int>("The longest river in the world is the", "Nile", 10);
            yield return new Tuple<string, string, int>("The longest highway in the world is the", "Trans-Canada", 10);
            yield return new Tuple<string, string, int>("The longest highway in the world has a length of", "About 8000 km", 10);
            yield return new Tuple<string, string, int>("The highest mountain in the world is the", "Mount Everest", 10);
            yield return new Tuple<string, string, int>("The country that accounts for nearly one third of the total teak production of the world is", "Myanmar", 10);
            yield return new Tuple<string, string, int>("The biggest desert in the world is the", "Sahara desert", 10);
            yield return new Tuple<string, string, int>("The largest coffee growing country in the world is", "Brazil", 10);
            yield return new Tuple<string, string, int>("The country also known as 'country of copper' is", "Zambia", 10);
            yield return new Tuple<string, string, int>("The name given to the border which separates Pakistan and Afghanistan is", "Durand line", 10);
            yield return new Tuple<string, string, int>("The river Volga flows out into the", "Caspian sea", 10);
            yield return new Tuple<string, string, int>("The coldest place on the earth is", "Verkoyansk in Siberia", 10);
            yield return new Tuple<string, string, int>("The country which ranks second in terms of land area is", "Canada", 10);
            yield return new Tuple<string, string, int>("The largest Island in the Mediterranean sea is", "Sicily", 10);
            yield return new Tuple<string, string, int>("The river Jordan flows out into the", "Dead sea", 10);
            yield return new Tuple<string, string, int>("The biggest delta in the world is the", "Ganges Delta", 10);
            yield return new Tuple<string, string, int>("The capital city that stands on the river Danube is", "Belgrade", 10);
            yield return new Tuple<string, string, int>("The Japanese call their country as", "Nippon", 10);
            yield return new Tuple<string, string, int>("The length of the English channel is", "564 kilometres", 10);
            yield return new Tuple<string, string, int>("The world's oldest known city is", "Damascus", 10);
            yield return new Tuple<string, string, int>("The city which is also known as the City of Canals is", "Venice", 10);
            yield return new Tuple<string, string, int>("The country in which river Wangchu flows is", "Myanmar", 10);
            yield return new Tuple<string, string, int>("The biggest island of the world is", "Greenland", 10);
            yield return new Tuple<string, string, int>("The city which is the biggest centre for manufacture of automobiles in the world is", "Detroit, USA", 10);
            yield return new Tuple<string, string, int>("The country which is the largest producer of manganese in the world is", "China & South Africa", 10);
            yield return new Tuple<string, string, int>("The country which is the largest producer of rubber in the world is", "Malaysia", 10);
            yield return new Tuple<string, string, int>("The country which is the largest producer of tin in the world is", "China", 10);
            yield return new Tuple<string, string, int>("The river which carries maximum quantity of water into the sea is the", "Amazon River", 10);
            yield return new Tuple<string, string, int>("The city which was once called the `Forbidden City' was", "Peking", 10);
            yield return new Tuple<string, string, int>("The country called the Land of Rising Sun is", "Japan", 10);
            yield return new Tuple<string, string, int>("Mount Everest was named after", "Sir George Everest", 10);
            yield return new Tuple<string, string, int>("The volcano Vesuvius is located in", "Italy", 10);
            yield return new Tuple<string, string, int>("The country known as the Sugar Bowl of the world is", "Cuba", 10);
            yield return new Tuple<string, string, int>("The length of the Suez Canal is", "162.5 kilometers", 10);
            yield return new Tuple<string, string, int>("The lowest point on earth is", "The coastal area of Dead sea", 10);
            yield return new Tuple<string, string, int>("The Gurkhas are the original inhabitants of", "Nepal", 10);
            yield return new Tuple<string, string, int>("The largest ocean of the world is the", "Pacific ocean", 10);
            yield return new Tuple<string, string, int>("The largest bell in the world is the", "Tsar Kolkol at Kremlin, Moscow", 10);
            yield return new Tuple<string, string, int>("The biggest stadium in the world is the", "Strahov Stadium, Prague", 10);
            yield return new Tuple<string, string, int>("The world's largest diamond producing country is", "South Africa", 10);
            yield return new Tuple<string, string, int>("Australia was discovered by", "James Cook", 10);
            yield return new Tuple<string, string, int>("The first Governor General of Pakistan is", "Mohammed Ali Jinnah", 10);
            yield return new Tuple<string, string, int>("Dublin is situated at the mouth of river", "Liffey", 10);
            yield return new Tuple<string, string, int>("The earlier name of New York city was", "New Amsterdam", 10);
            yield return new Tuple<string, string, int>("The Eiffel tower was built by", "Alexander Eiffel", 10);
            yield return new Tuple<string, string, int>("The Red Cross was founded by", "Jean Henri Durant", 10);
            yield return new Tuple<string, string, int>("The country which has highest population density is", "Monaco", 10);
            yield return new Tuple<string, string, int>("The national flower of Britain is", "Rose", 10);
            yield return new Tuple<string, string, int>("Niagara Falls was discovered by", "Louis Hennepin", 10);
            yield return new Tuple<string, string, int>("The national flower of Italy is", "Lily", 10);
            yield return new Tuple<string, string, int>("The national flower of China is", "Narcissus", 10);
            yield return new Tuple<string, string, int>("The permanent secretariat of the SAARC is located at", "Kathmandu", 10);
            yield return new Tuple<string, string, int>("The gateway to the Gulf of Iran is", "Strait of Hormuz", 10);
            yield return new Tuple<string, string, int>("The first Industrial Revolution took place in", "England", 10);
            yield return new Tuple<string, string, int>("World Environment Day is observed on", "5th June", 10);
            yield return new Tuple<string, string, int>("The first Republican President of America was", "Abraham Lincoln", 10);
            yield return new Tuple<string, string, int>("The country famous for Samba dance is", "Brazil", 10);
            yield return new Tuple<string, string, int>("The name of Alexander's horse was", "Beucephalus", 10);
            yield return new Tuple<string, string, int>("Singapore was founded by", "Sir Thomas Stamford Raffles", 10);
            yield return new Tuple<string, string, int>("The famous British one-eyed Admiral was", "Nelson", 10);
            yield return new Tuple<string, string, int>("The earlier name of Sri Lanka was", "Ceylon", 10);
            yield return new Tuple<string, string, int>("The UNO was formed in the year", "1945", 10);
            yield return new Tuple<string, string, int>("UNO stands for", "United Nations Organization", 10);
            yield return new Tuple<string, string, int>("The independence day of South Korea is celebrated on", "15th August", 10);
            yield return new Tuple<string, string, int>("'Last Judgement' was the first painting of an Italian painter named", "Michelangelo", 10);
            yield return new Tuple<string, string, int>("Paradise Regained was written by", "John Milton", 10);
            yield return new Tuple<string, string, int>("The first President of Egypt was", "Mohammed Nequib", 10);
            yield return new Tuple<string, string, int>("The first man to reach North Pole was", "Rear Admiral Robert E. Peary", 10);
            yield return new Tuple<string, string, int>("The most famous painting of Pablo Picasso was", "Guermica", 10);
            yield return new Tuple<string, string, int>("The primary producer of newsprint in the world is", "Canada", 10);
            yield return new Tuple<string, string, int>("The first explorer to reach the South Pole was", "Cap. Ronald Amundson", 10);
            yield return new Tuple<string, string, int>("The person who is called the father of modern Italy is", "G.Garibaldi", 10);
            yield return new Tuple<string, string, int>("World literacy day is celebrated on", "8th September", 10);
            yield return new Tuple<string, string, int>("The founder of modern Germany is", "Bismarck", 10);
            yield return new Tuple<string, string, int>("The country known as the land of the midnight sun is", "Norway", 10);
            yield return new Tuple<string, string, int>("The place known as the Roof of the world is", "Tibet", 10);
            yield return new Tuple<string, string, int>("The founder of the Chinese Republic was", "San Yat Sen", 10);
            yield return new Tuple<string, string, int>("The first Pakistani to receive the Nobel Prize was", "Abdul Salam", 10);
            yield return new Tuple<string, string, int>("The first woman Prime Minister of Britain was", "Margaret Thatcher", 10);
            yield return new Tuple<string, string, int>("The first Secretary General of the UNO was", "Trygve Lie", 10);
            yield return new Tuple<string, string, int>("The sculptor of the statue of Liberty was", "Frederick Auguste Bartholdi", 10);
            yield return new Tuple<string, string, int>("The port of Baku is situated in", "Azerbaijan", 10);
            yield return new Tuple<string, string, int>("John F Kennedy was assassinated by", "Lee Harvey Oswald", 10);
            yield return new Tuple<string, string, int>("The largest river in France is", "Loire", 10);
            yield return new Tuple<string, string, int>("The Queen of England who married her brother-in-law was", "Catherine of Aragon", 10);
            yield return new Tuple<string, string, int>("The first black person to be awarded the Nobel Peace Prize was", "Ralph Johnson Bunche", 10);
            yield return new Tuple<string, string, int>("The first British University to admit women for degree courses was", "London University", 10);
            yield return new Tuple<string, string, int>("The principal export of Jamaica is", "Sugar", 10);
            yield return new Tuple<string, string, int>("New York is popularly known as the city of", "Skyscrapers", 10);
            yield return new Tuple<string, string, int>("Madagascar is popularly known as the Island of", "Cloves", 10);
            yield return new Tuple<string, string, int>("The country known as the Land of White Elephant is", "Thailand", 10);
            yield return new Tuple<string, string, int>("The country known as the Land of Morning Calm is", "Korea", 10);
            yield return new Tuple<string, string, int>("The country known as the Land of Thunderbolts is", "Bhutan", 10);
            yield return new Tuple<string, string, int>("The highest waterfalls in the world is the", "Salto Angel Falls, Venezuela", 10);
            yield return new Tuple<string, string, int>("The largest library in the world is the", "United States Library of Congress, Washington DC", 10);
            yield return new Tuple<string, string, int>("The author of Harry Potter Books is", "JK Rowling", 10);
            yield return new Tuple<string, string, int>("Nickname of New York city is", "Big Apple", 10);
            yield return new Tuple<string, string, int>("What do you call a group of sheep?", "A Flock of Sheep", 10);
            yield return new Tuple<string, string, int>("In which sport do players take long and short corners?", "Hockey", 10);
            yield return new Tuple<string, string, int>("Who was the youngest President of the USA?", "Theodore Roosevelt", 10);
            yield return new Tuple<string, string, int>("How many legs do butterflies have?", "6 Legs & 2 Pair of Wings", 10);
            yield return new Tuple<string, string, int>("Who invented the Nintendo Wii?", "Kashi Kabushiki", 10);
            yield return new Tuple<string, string, int>("What year does the Nintendo Wii come out?", "Late 2006", 10);
            yield return new Tuple<string, string, int>("Who invented the Light Bulb?", "Humphry Davy", 10);
            yield return new Tuple<string, string, int>("Who invented the washing machine?", "James King", 10);
            yield return new Tuple<string, string, int>("Who invented the first electric washing machine?", "Alva Fisher", 10);
            yield return new Tuple<string, string, int>("Who invented the safety pin?", "Walter Hunt", 10);
            yield return new Tuple<string, string, int>("Who invented the Vacuum Cleaner?", "Hubert Booth", 10);
            yield return new Tuple<string, string, int>("Who won the Football World Cup in 2006?", "Italy", 10);
            yield return new Tuple<string, string, int>("Which country hosted the Football World Cup in 2006?", "Germany", 10);
            yield return new Tuple<string, string, int>("Who is the new Prime Minister of the United Kingdom?", "David Cameron", 10);
            yield return new Tuple<string, string, int>("Who won Men's Singles title in French Open 2010?", "Rafael Nadal (Spain)", 10);
            yield return new Tuple<string, string, int>("Who won Women's Singles title in French Open 2010?", "Francesca Schiavone (Italy)", 10);
            yield return new Tuple<string, string, int>("Who won Men's Doubles title in French Open 2010?", "Daniel Nestor (Canada) & Nenad Zimonjic (Serbia)", 10);
            yield return new Tuple<string, string, int>("Who won Women's Doubles title in French Open 2010?", "Serena & Venus Williams (USA)", 10);
            yield return new Tuple<string, string, int>("What is the longest word in English in which each letter is used at least two times?", "Unprosperousness", 10);
            yield return new Tuple<string, string, int>("What is the most popular breed of dog?", "Retrievers", 10);
            yield return new Tuple<string, string, int>("Who is the CEO of search company Google?", "Eric Schmidt", 10);
            yield return new Tuple<string, string, int>("Who is Miss USA 2010?", "Rima Fakih", 10);
            yield return new Tuple<string, string, int>("Which country won the Thomas Cup title for Badminton in 2010?", "China", 10);
            yield return new Tuple<string, string, int>("Who was the first Indian to join the Indian Civil Services?", "Satyendranath Tagore", 10);
            yield return new Tuple<string, string, int>("Who was the first woman Governor of India?", "Sarojini Naidu", 10);
            yield return new Tuple<string, string, int>("Which two countries have signed the Nuclear Swap deal with Iran?", "Brazil and Turkey", 10);
            yield return new Tuple<string, string, int>("Who won the Madrid Masters men's tournament in 2010?", "Rafael Nadal", 10);
            yield return new Tuple<string, string, int>("Who is CEO of Yahoo?", "Carol Bartz", 10);
            yield return new Tuple<string, string, int>("Who is the first man to climb Mount Everest without oxygen?", "Phu Dorji", 10);
            yield return new Tuple<string, string, int>("How many words can you make from a five letter word by shuffling the places of each alphabet?", "120", 10);
            yield return new Tuple<string, string, int>("Speed of computer mouse is measured in which unit?", "Mickey", 10);
            yield return new Tuple<string, string, int>("Who topped Forbes list of 'Billionaire Universities' in 2010?", "Harvard University", 10);
            yield return new Tuple<string, string, int>("Barack Obama's birthday is on which date?", "August 4, 1961", 10);
            yield return new Tuple<string, string, int>("Which bird is the international symbol of happiness?", "Bluebird", 10);
            yield return new Tuple<string, string, int>("What is the pirate's flag with the skull and cross-bones called?", "Jolly Roger", 10);
            yield return new Tuple<string, string, int>("What is the common name for ascorbic acid?", "Vitamin C", 10);
            yield return new Tuple<string, string, int>("Which useful household item is made from naphthalene?", "Mothballs", 10);


        }
        #endregion
        public override IEnumerable<string> SupportedInitiators
        {
            get
            {
                yield return "quiz";
            }
        }

        public override IEnumerable<string> SupportedCommands
        {
            get
            {
                yield return "celeb";
                yield return "gk";
                yield return "answer";
                yield return "info";
                yield return "help";
                yield return "score";
                yield return "top10";
                yield return "topten";
            }
        }

        public override bool ExecuteCommand()
        {
            switch (Command)
            {
                case "celeb":
                    return AskQuestion(QuestionType.Celebrity);

                case "gk":
                    return AskQuestion(QuestionType.GeneralKnowledge);

                case "answer":
                    return VerifyAnswer(PassAnswer());

                case "score":
                    return ShowScore();

                case "topten":
                case "top10":
                    return ShowTop10();


                case "info":
                case "help":
                    return ExecuteShowInfo();

                default:
                    return false;
            }
        }

    	public override string SprocketName
    	{
    		get { return "Quiz Sprocket"; }
    	}

    	private bool ShowTop10()
        {
            var stringBuilder = new StringBuilder();

            var orderedScores = _scoreTable.Select(t => new { Name = t.Key, Score = t.Value }).OrderByDescending(t => t.Score).Take(10).ToList();

            if (orderedScores.Count == 0) { Bot.Say("No scores recorded", Message.Receiver); return true; }

            stringBuilder.AppendLine("Scoretable:");

            for (int index = 0; index < orderedScores.Count; index++)
            {
                var orderedScore = orderedScores[index];
                stringBuilder.AppendLine(string.Format("{0,2} - {1} {2}", index + 1, orderedScore.Name.PadRight(15), orderedScore.Score));
            }

            Bot.Say(stringBuilder.ToString(), Message.Receiver);

            return true;
        }

        private bool ShowScore()
        {
            int currentScore;

            Bot.Say(
                _scoreTable.TryGetValue(Message.Sender, out currentScore)
                    ? string.Format("{0} has a score of {1}", Message.Sender, currentScore)
                    : string.Format("No score recorded for {0}", Message.Sender), Message.Receiver);

            return true;
        }

        private bool VerifyAnswer(string answer)
        {
            var longestCommonSubstringLength = LongestCommonSubstring(answer, _lastAskedQuestion.Item2);
            var levenshteinDistance = LevenshteinDistance(answer, _lastAskedQuestion.Item2);
            var correct = _lastAskedQuestion != null
                            && (answer.Equals(_lastAskedQuestion.Item2, StringComparison.InvariantCultureIgnoreCase)
                            || (longestCommonSubstringLength / (decimal)_lastAskedQuestion.Item2.Length) > CorrectnessFraction
                            || (levenshteinDistance / (decimal)_lastAskedQuestion.Item2.Length) < (1 - CorrectnessFraction)); 

            if (correct)
            {
                Bot.Say(string.Format("Correct {0}, the answer is {1}. That was brilliant!", Message.Sender, _lastAskedQuestion.Item2), Message.Receiver);
                int currentScore = 0;

                if (_scoreTable.TryGetValue(Message.Sender, out currentScore))
                {
                    currentScore += _lastAskedQuestion.Item3;
                    _scoreTable[Message.Sender] = currentScore;
                }
                else
                    _scoreTable.Add(Message.Sender, _lastAskedQuestion.Item3);

                _lastAskedQuestion = null;
            }
            else
                Bot.Say(string.Format("Wrong!"), Message.Receiver);


            return correct;
        }

        private string PassAnswer()
        {
            var answer = Message.Content.Substring(Message.Content.LastIndexOf(Command, StringComparison.Ordinal) + Command.Length).Trim();

            return answer;
        }

        private bool ExecuteShowInfo()
        {
            Bot.PrivateReply(Message.Sender, string.Format(_helpInfo, Message.Sender));

            return true;
        }

        private bool AskQuestion(QuestionType questionType)
        {
            if (_lastQuestionAskedAt.AddMinutes(1) <= DateTime.Now)
                _lastAskedQuestion = null;

            if (_lastAskedQuestion != null)
                Bot.PrivateReply("A question is currently waiting for the correct answer", Message.Receiver);
            else
                Bot.Say(GetQuestion(questionType), Message.Receiver);

            return true;
        }

        private string GetQuestion(QuestionType questionType)
        {
            var posibleQuestions = _questions[questionType]().ToList();

            _lastAskedQuestion = posibleQuestions[_random.Next(0, posibleQuestions.Count - 1)];
            _lastQuestionAskedAt = DateTime.Now;
            return _lastAskedQuestion.Item1;
        }

        #region String Algorithms
        private int LongestCommonSubstring(string str1, string str2)
        {
            if (String.IsNullOrEmpty(str1) || String.IsNullOrEmpty(str2))
                return 0;

            int[,] num = new int[str1.Length, str2.Length];
            int maxlen = 0;

            for (int i = 0; i < str1.Length; i++)
            {
                for (int j = 0; j < str2.Length; j++)
                {
                    if (str1[i] != str2[j])
                        num[i, j] = 0;
                    else
                    {
                        if ((i == 0) || (j == 0))
                            num[i, j] = 1;
                        else
                            num[i, j] = 1 + num[i - 1, j - 1];

                        if (num[i, j] > maxlen)
                        {
                            maxlen = num[i, j];
                        }
                    }
                }
            }
            return maxlen;
        }

        private static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
        #endregion
    }
}
