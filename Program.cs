/*
	A (hacky) port from Python: https://github.com/jezhiggins/eliza.py/blob/master/eliza.py
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace c__eliza
{
class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> reflections = new Dictionary<string, string>
            {
                ["am"] = "are",
                ["was"] = "were",
                ["i"] = "you",
                ["i'd"] = "you would",
                ["i've"] = "you have",
                ["i'll"] = "you will",
                ["my"] = "your",
                ["are"] = "am",
                ["you've"] = "I have",
                ["you'll"] = "I will",
                ["your"] = "my",
                ["yours"] = "mine",
                ["you"] = "me",
                ["me"] = "you"
            };

            List<List<String>> responses = setupResponses();

            Console.Clear();
            Console.WriteLine("Therapist\n---------");
            Console.WriteLine("Talk to the program by typing in plain English, using normal upper-");
            Console.WriteLine("and lower-case letters and punctuation.  Enter 'quit' when done.");
            Console.WriteLine(new String('=', 72));
            Console.WriteLine("Hello.  How are you feeling today?");

            bool exit = false;

            Random rnd = new Random();

            while (!exit)
            {
                string chosenResponse = "";
                string userInput = Console.ReadLine().ToLower();
                // remove punctuation at end of input
                userInput = Regex.Replace(userInput, @"[^\w\s]{1,}$", "");

                // test all regexes in turn
                foreach (List<String> possibleResponses in responses)
                {
                    // right to left as we only need the last match
                    Regex pattern = new Regex(possibleResponses[0]);
                    // got a bite?
                    if (pattern.IsMatch(userInput))
                    {
                        // pick a random response
                        int randomIdx = rnd.Next(1, possibleResponses.Count);
                        chosenResponse = possibleResponses[randomIdx];

                        // if there's a %1, we need to include part of what the user said
                        if (chosenResponse.Contains("%1"))
                        {
                            // get the bit of the text we're going to include
                            GroupCollection matchedTextGroups = pattern.Match(userInput).Groups;
                            string reflectedInput = "";
                            foreach (Group g in matchedTextGroups)
                            {
                                reflectedInput = g.ToString();
                            }
                            // check all words, flip using reflections (i.e. I am -> you are)
                            String[] reflectedInputArray = reflectedInput.Split();
                            for (int i = 0; i < reflectedInputArray.Length; i++)
                            {
                                // element and potential dictionary key are different
                                string word = reflectedInputArray[i];
                                // strip punctuation so "am," still becomes "are,"
                                string possibleKey = Regex.Replace(word, @"\p{P}", "");
                                if (reflections.ContainsKey(possibleKey))
                                {
                                    word = word.Replace(possibleKey, reflections[possibleKey]);
                                    reflectedInputArray[i] = word;
                                }
                            }
                            // join it up again and stick the last matched group into the placeholder
                            reflectedInput = String.Join(" ", reflectedInputArray);
                            chosenResponse = chosenResponse.Replace("%1", reflectedInput);
                            // strip duplicate punctuation
                            chosenResponse = Regex.Replace(chosenResponse, @"([^\w\s])(\1){1,}", @"$2");
							// capitalize first letter
							chosenResponse = Char.ToUpper(chosenResponse[0]).ToString() + chosenResponse.Substring(1);
                        }
                        break;
                    }
                }

                Console.WriteLine(chosenResponse);
                if (userInput == "quit")
                {
                    exit = true;
                }
            }
        }

        static List<List<string>> setupResponses()
        {
            // list of lists - first option in each sub-list is the regex to match,
            // the rest are possible responses
			List<List<String>> allResponses = new List<List<string>>();
            using (StreamReader inputStream = File.OpenText("./data/responses.txt"))
            {
                string txtLine = "";
                List<String> tempResponseCollector = new List<string>();
                while ((txtLine = inputStream.ReadLine()) != null)
                {
                    if (txtLine == "-")
                    {
                        allResponses.Add(tempResponseCollector);
						// .Clear() gives some odd results here
                        tempResponseCollector = new List<string>();
                    }
                    else
                    {
                        tempResponseCollector.Add(txtLine.ToString());
                    }
                }
            }

			return allResponses;
        }
    }
}
