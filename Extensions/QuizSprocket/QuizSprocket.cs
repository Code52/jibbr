using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jabbot;
using Jabbot.CommandSprockets;
using Jabbot.Models;
using Jabbot.Sprockets.Core;

namespace Jabbot.Extensions
{
    public class QuizSprocket : CommandSprocket
    {
        private enum QuestionType { Celebrity } ;
        private readonly string _helpInfo;
        private readonly Random _random;
        private readonly Dictionary<QuestionType, Func<IEnumerable<Tuple<string, string, int>>>> _questions;
        private readonly Dictionary<string, int> _scoreTable;
        private Tuple<string, string, int> _lastAskedQuestion;
        private DateTime _lastQuestionAskedAt;

        public QuizSprocket()
        {
            _helpInfo = "Hi {0}," + Environment.NewLine + Environment.NewLine
                        + "I accept the following commands:"
                        + "celeb:\t" + "request a new celebrity quiz question" + Environment.NewLine
                        + "answer:\t" + "answer the last asked question" + Environment.NewLine
                        + "score:\t" + "see your  the last asked question" + Environment.NewLine
                        + "top10/topten:\t" + "see the top10 of the scoretable" + Environment.NewLine
                        + "info/help:\t" + "this message" + Environment.NewLine;

            _random = new Random();

            CelebrityQuestions = InitializeCelebrityQuestions();


            _questions = new Dictionary<QuestionType, Func<IEnumerable<Tuple<string, string, int>>>>
                             {
                                 {QuestionType.Celebrity, () => CelebrityQuestions}
                             };

            _scoreTable = new Dictionary<string, int>();
        }

        public IEnumerable<Tuple<string, string, int>> CelebrityQuestions { get; set; }

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
            yield return new Tuple<string, string, int>("Academy Award-Winning Actor Jon Voight is the father of which famous Actress?", "Katherine Hepburn", 10);
            yield return new Tuple<string, string, int>("Which actor/actress has won the most Oscars?", "Angelina Jolie", 10);
            yield return new Tuple<string, string, int>("What is Nicholas Cage's real name?", "Nicolas Kim Coppola", 10);
            yield return new Tuple<string, string, int>("Who Am I?? Musician/Actor born in 1968 in Philadelphia. Starred in popular teen comedy and has been known to be a bit of a 'Bad Boy' and self-proclaimed 'Legend'.....", "Will Smith", 10);
            yield return new Tuple<string, string, int>("In which Southern US city was Pop Superstar Britney Spears born and raised?", "Kentwood, Louisiana", 10);
            yield return new Tuple<string, string, int>("Which celebrity couple called their baby 'Apple'?", "Gwyneth Paltrow and Chris Martin", 10);
            yield return new Tuple<string, string, int>("Complete this movie title - 'While you were... ____', ", "Sleeping", 10);
            yield return new Tuple<string, string, int>("The Indian movie industry is better known as?", "Bollywood", 10);
        }

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
            var correct = _lastAskedQuestion != null && answer.Equals(_lastAskedQuestion.Item2, StringComparison.InvariantCultureIgnoreCase);

            if (correct)
            {
                Bot.Say(string.Format("Correct {0}, that was brilliant!", Message.Sender), Message.Receiver);
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
            var answer = Message.Content.Substring(Message.Content.LastIndexOf(Command) + Command.Length).Trim();

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
    }
}
