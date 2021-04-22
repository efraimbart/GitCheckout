using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace GitCheckout
{
    public class Choices<T> : List<Choices<T>.Choice>
    {
        public string Question { get; set; }
        
        public Choices(string question)
        {
            Question = question;
        }
        
        public Choices<T> Add(string text, T value)
        {
            Add(new Choice {Text = text, Value = value});
            return this;
        }

        public Choices<T> Add(T value)
        {
            return Add(value.ToString(), value);
        }

        public Choice Choose()
        {
            Console.WriteLine(Question);
            
            for (var i = 0; i < Count; i++)
            {
                var choice = this[i];
                Console.WriteLine($@"[{i + 1}] {choice.Text}");
            }

            var chosenString = Console.ReadLine();
            Console.WriteLine();

            var chosen = this.FirstOrDefault(x => x.Text.Equals(chosenString, StringComparison.InvariantCultureIgnoreCase));
            if (chosen != null)
            {
                return chosen;
            }
            
            if (!int.TryParse(chosenString, out var chosenNumber))
            {
                return null;
            }
            
            return this.ElementAtOrDefault(chosenNumber - 1);
        }

        public class Choice
        {
            public string Text { get; set; }
            public T Value { get; set; }
        }
    }

    public class Choices : Choices<string>
    {
        public Choices(string question, StringCollection collection) : base(question)
        {
            foreach (var choice in collection)
            {
                Add(choice);
            }
        }
    }
}