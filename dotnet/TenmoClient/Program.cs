using System;
using System.Collections.Generic;
using TenmoClient.Models;
using System.Linq;

namespace TenmoClient
{
    class Program
    {
        private static readonly ConsoleService consoleService = new ConsoleService();
        private static readonly AuthService authService = new AuthService();

        static void Main(string[] args)
        {
            Run();
        }

        private static void Run()
        {
            int loginRegister = -1;
            while (loginRegister != 1 && loginRegister != 2)
            {
                Console.WriteLine("Welcome to TEnmo!");
                Console.WriteLine("1: Login");
                Console.WriteLine("2: Register");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out loginRegister))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else if (loginRegister == 1)
                {
                    while (!UserService.IsLoggedIn()) //will keep looping until user is logged in
                    {
                        LoginUser loginUser = consoleService.PromptForLogin();
                        ApiUser user = authService.Login(loginUser);
                        if (user != null)
                        {
                            UserService.SetLogin(user);
                        }
                    }
                }
                else if (loginRegister == 2)
                {
                    bool isRegistered = false;
                    while (!isRegistered) //will keep looping until user is registered
                    {
                        LoginUser registerUser = consoleService.PromptForLogin();
                        isRegistered = authService.Register(registerUser);
                        if (isRegistered)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Registration successful. You can now log in.");
                            loginRegister = -1; //reset outer loop to allow choice for login
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                }
            }

            MenuSelection();
        }

        private static void MenuSelection()
        {
            int menuSelection = -1;
            while (menuSelection != 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Welcome to TEnmo! Please make a selection: ");
                Console.WriteLine("1: View your current balance");
                Console.WriteLine("2: View your past transfers");
                Console.WriteLine("3: View your pending requests");
                Console.WriteLine("4: Send TE bucks");
                Console.WriteLine("5: Request TE bucks");
                Console.WriteLine("6: Log in as different user");
                Console.WriteLine("0: Exit");
                Console.WriteLine("---------");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out menuSelection))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else if (menuSelection == 1)
                {
                    try
                    {
                        Console.WriteLine("Your current account balance is: $" + authService.GetBalance());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (menuSelection == 2)
                {
                    Console.WriteLine("---------\nTransfers\n");
                    //Console.WriteLine("Transfers\nID\t From/To\t Amount");
                    //TODO-------FIX THE FORMATTING
                    Console.WriteLine("{0,-10} | {1,-10} | {2,10}", "ID", "From/To", "Amount");
                    Console.WriteLine("---------");
                    List<Transaction> transactions = authService.GetTransactions();
                    foreach (Transaction item in transactions)
                    {
                        if (item.FromUserId == UserService.GetUserId())
                        {
                            Console.WriteLine("ID: {0,-10} To: {1,-10} {2,5:C}", item.Id, item.ToUserName, item.Amount);
                        }
                        else
                        {
                            Console.WriteLine("ID: {0,-10} From: {1,-10} {2,5:C}", item.Id, item.FromUserName, item.Amount);
                        }
                    }
                    Console.WriteLine("---------");
                    Console.WriteLine("Select a transaction via its ID you would like more information on (Enter '0' for previous menu): ");
                    string selectionAsString = Console.ReadLine();
                    int selection = 1;
                    Transaction transaction = new Transaction();

                    //Loops while checking if transaction or its Id is null
                    //TODO----Make this into a method to verify if something exists and loops until verified sucessfully
                    while (transaction == null || transaction.Id == null)
                    {
                        //Returns the user back to the main menu if the input is '0'
                        if (selectionAsString == "0")
                        {
                            break;
                        }
                        //Attempts to parse the string input to an int
                        if (int.TryParse(selectionAsString, out selection))
                        {
                            //Attempts to set transaction to a transaction in the list where the user input matches the Id of the transaction
                            //If the Id is not found, it will make transaction either null or create a transaction object but all of its properties are null
                            transaction = transactions.FirstOrDefault(x => x.Id == selection);
                            if (transaction == null || transaction.Id == null)
                            {
                                Console.WriteLine("Please enter a valid selection of an ID from above (Enter '0' for previous menu):");
                                selectionAsString = Console.ReadLine();
                            }
                        }
                        //Catches if the input can not be parsed as an int
                        else
                        {
                            Console.WriteLine("Please enter a valid selection of an ID from above (Enter '0' for previous menu):");
                            selectionAsString = Console.ReadLine();
                        }
                    }
                    if (selectionAsString != "0")
                    {
                        Console.WriteLine("---------");
                        Console.WriteLine("Transaction Details");
                        Console.WriteLine("---------");
                        Console.WriteLine();
                        Console.WriteLine($"Transaction ID: {transaction.Id}\n From: {transaction.FromUserName}\n To: {transaction.ToUserName}\n Transaction Type: {transaction.Type}\n Transaction Status: {transaction.Status}\n Amount Transferred: ${transaction.Amount}");
                    }
                    
                }

                else if (menuSelection == 3)
                {
                    Console.WriteLine("---------");
                    Console.WriteLine("Transfers\nID\t From/To\t Amount");
                    Console.WriteLine("---------");
                    List<Transaction> transactions = authService.GetPendingTransactions();
                    foreach (Transaction item in transactions)
                    {
                        if (item.FromUserId == UserService.GetUserId())
                        {
                            Console.WriteLine($"ID:{item.Id}\t To: {item.ToUserName}\t ${item.Amount}");
                        }
                        else
                        {
                            Console.WriteLine($"ID:{item.Id}\t From: {item.FromUserName}\t ${item.Amount}");
                        }
                    }
                    Console.WriteLine("---------");
                    Console.WriteLine("Select the ID of the transaction you would like to complete/reject: ");
                    string transactionIdAsString = Console.ReadLine();
                    int transactionId = 0;
                    int.TryParse(transactionIdAsString, out transactionId);
                    Console.WriteLine("---------");
                    Console.WriteLine("Would you like to Complete(1) or Reject(2) the request? ");
                    string completionOptionAsString = Console.ReadLine();
                    int completionOption = int.Parse(completionOptionAsString);
                    Transaction transaction = transactions.FirstOrDefault(x => x.Id == transactionId);
                    if (transaction != null && (completionOption == 1 || completionOption == 2))
                    {
                        Console.WriteLine(authService.CompleteTransaction(transactionId, completionOption));
                        
                    }
                    else
                    {
                        Console.WriteLine("Please select an ID that is actually existent or either 1 or 2 for your completion option.");
                    }
                }
                else if (menuSelection == 4)
                {
                    try
                    {
                        List<User> listOfUsers = authService.GetUsers();
                        foreach (User user in listOfUsers)
                        {
                            if (user.UserId != UserService.GetUserId())
                            {
                                Console.WriteLine();
                                Console.WriteLine($"ID:{user.UserId} , {user.Username}");
                            }
                        }
                        Console.WriteLine();
                        Console.WriteLine("---------");
                        Console.WriteLine("Enter the ID of the person you would like to send money to: ");
                        string receiverIdString = Console.ReadLine();
                        int requesterId = int.Parse(receiverIdString);

                        //cannot send money to yourself
                        if (requesterId == UserService.GetUserId())
                        {
                            Console.WriteLine("You are unable to send money to yourself, please select a valid Id.");
                        }
                        else
                        {
                            //Display error message if amount sent is more than balance
                            Console.WriteLine("How much money would you like to send? ");
                            string amountAsString = Console.ReadLine();
                            decimal amount = decimal.Parse(amountAsString);
                            if(amount > 0)
                            {
                                Console.WriteLine("---------");
                                Console.WriteLine(authService.SendMoney(requesterId, amount));
                            }
                            else
                            {
                                Console.WriteLine("Please enter an amount greater than 0");
                            }

                        }
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine(e.Message);
                    }
                }
                else if (menuSelection == 5)
                {
                    //request method in transactiondao, transactioncontroller,authservice
                    //set to pending, refer to menuselect 3 to approve/reject
                    try
                    {
                        List<User> listOfUsers = authService.GetUsers();
                        foreach (User user in listOfUsers)
                        {
                            if (user.UserId != UserService.GetUserId())
                            {
                                Console.WriteLine();
                                Console.WriteLine($"ID:{user.UserId} , {user.Username}");
                            }
                        }
                        Console.WriteLine();
                        Console.WriteLine("---------");
                        Console.WriteLine("Enter the ID of the person you would like to request money from: ");
                        string receiveeIdString = Console.ReadLine();
                        int requesteeId = int.Parse(receiveeIdString);
                        if (requesteeId == UserService.GetUserId())
                        {
                            Console.WriteLine("You are unable to request money from yourself, please select a valid Id.");
                        }
                        else
                        {
                            //Display error message if amount sent is more than balance
                            Console.WriteLine("How much money would you like to request? ");
                            string amountAsString = Console.ReadLine();
                            decimal amount = decimal.Parse(amountAsString);
                            if (amount > 0)
                            {
                                Console.WriteLine("---------");
                                Console.WriteLine(authService.RequestMoney(requesteeId, amount));
                            }else
                            {
                                Console.WriteLine("Please enter an amount greater than 0");
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (menuSelection == 6)
                {
                    Console.WriteLine("");
                    UserService.SetLogin(new ApiUser()); //wipe out previous login info
                    Run(); //return to entry point
                }
                else
                {
                    Console.WriteLine("Goodbye!");
                    Environment.Exit(0);
                }
            }
        }
    }
}
