using System;
using System.IO;

namespace PowerConsole.Test
{
    public static class UserInfoCollector
    {
        // create a new instance of SmartConsole to avoid
        // interference with previous console sessions;
        // the current culture is preserved
        static readonly SmartConsole MyConsole = new SmartConsole();

        public static SmartConsole Process()
        {
            MyConsole.WriteInfo("\nWelcome to the user info collection demo!\n");

            // by simply providing a validation message, we force 
            // the input not to be empty or white space only (and to
            // be of the appropriate type if different from string)
            var nameValidationMessage = "Your full name is required: ";

            bool ageValidator(int input) => input >= 5 && input <= 100;
            var ageErrorMessage = "Age (in years) must be a whole number from 5 to 100: ";

            // notice the 'promptId' parameter: they'll allow us 
            // strongly-typed object instantiation and property mapping
            while
            (
                MyConsole.Store() // forces all subsequent prompts to be stored
                    .Prompt("\nEnter your full name: ", "Full Name:", validationMessage: nameValidationMessage, promptId: nameof(UserInfo.FullName))
                    .Prompt<int>("How old are you? ", "Plain Age:", validationMessage: ageErrorMessage, validator: ageValidator, promptId: nameof(UserInfo.Age))
                    .Prompt("In which country were you born? ", "Birth Country:", promptId: nameof(UserInfo.BirthCountry))
                    .Prompt("What's your preferred color? ", "Preferred Color:", promptId: nameof(UserInfo.PreferredColor))
                    .WriteLine()
                    .WriteLine("Here's what you've entered: ")
                    .WriteLine()
                    .Recall(prefix: "> ")
                    .WriteLine()
                    .Store(false) // stops storing prompts

                    // give the user an opportunity to review and correct their inputs
                    .PromptYes("Is that correct? (Y/n) ") == false
            )
            {
                // nothing else required within this while loop
            }

            MyConsole.WriteInfo("Thank you for providing your details.\n");

            if (!MyConsole.PromptNo("Do you wish to save them now? (y/N) "))
            {
                SaveUserDetails();
            }

            return MyConsole;
        }

        static void SaveUserDetails()
        {
            // now process the collected data; obviously, you'll have to 
            // save it to a useful and secure store somehow...
            MyConsole
                .WriteLine()
                .Repeat('-', 70)
                .WriteWarning("CAUTION: EXISTING FILES WILL BE OVERRIDDEN WITHOUT FURTHER NOTICE!\n")
                .Repeat('-', 70)
                .WriteLine();

            // but for the sake of simplicity, we'll save them to disk
            var filename = MyConsole.GetResponse("File name (full path); leave empty to use current directory: ");

            string path;

            if (string.IsNullOrWhiteSpace(filename))
            {
                path = Path.Combine(Environment.CurrentDirectory, $"{nameof(SmartConsole)}Log");
                filename = $"{path}-UserInfo.txt";
            }
            else
            {
                path = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
            }

            var historyFileName = $"{path}-History.txt";

            // create a UserInfo object from the previously-collected
            // AND identified prompts (created with the 'promptId' parameter)
            var user = MyConsole.CreateObject<UserInfo>();

            File.WriteAllText(filename, user.ToString());

            MyConsole
                // we can also save all prompts with their respective responses to a file
                .WriteFile(historyFileName)
                .WriteLine($"\nYour details have been saved to:\n{filename}\n\nand:\n{historyFileName}\n");
        }
    }
}
