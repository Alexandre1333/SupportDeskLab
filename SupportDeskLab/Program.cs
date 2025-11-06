using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using static SupportDeskLab.Utility;


namespace SupportDeskLab
{

    class Program
    {
        static int NextTicketId = 1;

        static Dictionary<string, Customer> Customers = new Dictionary<string, Customer>();
        static Queue<Ticket> Tickets = new Queue<Ticket>();
        static Stack<UndoEvent> UndoEvents = new Stack<UndoEvent>();

        static void Main()
        {
            initCustomer();

            while (true)
            {
                Console.WriteLine("\n=== Support Desk ===");
                Console.WriteLine("[1] Add customer");
                Console.WriteLine("[2] Find customer");
                Console.WriteLine("[3] Create ticket");
                Console.WriteLine("[4] Serve next ticket");
                Console.WriteLine("[5] List customers");
                Console.WriteLine("[6] List tickets");
                Console.WriteLine("[7] Undo last action");
                Console.WriteLine("[0] Exit");
                Console.Write("Choose: ");
                string choice = Console.ReadLine();


                switch (choice)
                {
                    case "1": AddCustomer(); break;
                    case "2": FindCustomer(); break;
                    case "3": CreateTicket(); break;
                    case "4": ServeNext(); break;
                    case "5": ListCustomers(); break;
                    case "6": ListTickets(); break;
                    case "7": Undo(); break;
                    case "0": return;
                    default: Console.WriteLine("Invalid option."); break;
                }
            }
        }

        static void initCustomer()
        {
            Customers["C001"] = new Customer("C001", "Ava Martin", "ava@example.com");
            Customers["C002"] = new Customer("C002", "Ben Parker", "ben@example.com");
            Customers["C003"] = new Customer("C003", "Chloe Diaz", "chloe@example.com");
        }

        static void AddCustomer()
        {
            Console.Write("Enter customer ID: ");
            string id = Console.ReadLine();

            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Customer ID cannot be empty.");
                return;
            }

            if (Customers.ContainsKey(id))
            {
                Console.WriteLine("Customer with this ID already exists.");
                return;
            }

            Console.Write("Enter customer name: ");
            string name = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Customer name cannot be empty.");
                return;
            }

            Console.Write("Enter customer email: ");
            string email = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("Customer email cannot be empty.");
                return;
            }

            var customer = new Customer(id, name, email);
            Customers[id] = customer;
            UndoEvents.Push(new UndoAddCustomer(customer));
            Console.WriteLine("Customer Added");
        }

        static void FindCustomer()
        {
            Console.WriteLine("Enter customer ID to find: ");
            string id = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Customer ID cannot be empty.");
                return;
            }

            if (Customers.TryGetValue(id, out var customer))
            {
                Console.Write("Customer found: ");
                Console.WriteLine(customer.ToString());
            }
            else
            {
                Console.WriteLine("Customer not found.");
            }

        }

        static void CreateTicket()
        {
            Console.WriteLine("Enter customer ID: ");
            string id = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Customer ID cannot be empty.");
                return;
            }

            if (!Customers.TryGetValue(id, out var customer))
            {
                Console.WriteLine("Customer not found.");
                return;
            }

            Console.WriteLine("Enter subject: ");
            string subject = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(subject))
            {
                Console.WriteLine("Subject cannot be empty.");
                return;
            }

            var ticket = new Ticket(NextTicketId++, id, subject);
            Tickets.Enqueue(ticket);
            UndoEvents.Push(new UndoCreateTicket(ticket));
            Console.Write("Ticket Created: ");
            Console.WriteLine(ticket.ToString());
        }

        static void ServeNext()
        {
            if (Tickets.Count == 0)
            {
                Console.WriteLine("No tickets to serve.");
                return;
            }

            var ticket = Tickets.Dequeue();
            UndoEvents.Push(new UndoServeTicket(ticket));
            Console.Write("Serving Ticket: ");
            Console.WriteLine(ticket.ToString());

        }

        static void ListCustomers()
        {
            Console.WriteLine("-- Customers --");

            if (Customers.Count == 0)
            {
                Console.WriteLine("No customers.");
                return;
            }

            foreach (var customer in Customers.Values)
            {
                Console.WriteLine(customer.ToString());
            }
        }

        static void ListTickets()
        {

            Console.WriteLine("-- Tickets (front to back) --");

            if (Tickets.Count == 0)
            {
                Console.WriteLine("No tickets.");
                return;
            }

            foreach (var ticket in Tickets)
            {
                Console.WriteLine(ticket.ToString());
            }

        }

        static void Undo()
        {
            if (UndoEvents.Count == 0)
            {
                Console.WriteLine("Nothing to undo.");
                return;
            }

            var undoEvent = UndoEvents.Pop();

            if (undoEvent is UndoAddCustomer undoAddCustomer)
            {
                var id = undoAddCustomer.Customer.CustomerId;

                if (Customers.Remove(id))
                {
                    Console.WriteLine("Undo: removed customer " + id);
                }

                else
                {
                    Console.WriteLine("Undo failed: customer not found.");
                }

            }

            else if (undoEvent is UndoCreateTicket undoCreateTicket)
            {

                var ticketID = undoCreateTicket.Ticket.TicketId;
                var newQueue = new Queue<Ticket>();
                bool removed = false;

                while (Tickets.Count > 0)
                {
                    var ticket = Tickets.Dequeue();
                    if (!removed && ticket.TicketId == ticketID)
                    {
                        removed = true;
                        continue;
                    }
                    newQueue.Enqueue(ticket);
                }

                Tickets = newQueue;
                if (removed)
                {
                    if (NextTicketId == ticketID + 1)
                    {
                        NextTicketId = ticketID;
                    }
                    Console.WriteLine("Undo: removed ticket #: " + ticketID);

                }

                else
                {
                    Console.WriteLine("Undo failed: ticket not found.");
                }
            }
            else if (undoEvent is UndoServeTicket undoServeTicket)
            {
                var ticket = undoServeTicket.Ticket;
                var newQueue = new Queue<Ticket>();
                newQueue.Enqueue(ticket);

                foreach (var t in Tickets)
                {
                    newQueue.Enqueue(t);
                }
                Tickets = newQueue;
                Console.WriteLine("Undo: restored served ticket #: " + ticket.TicketId);

            }
            
            else
            {
                Console.WriteLine("Unknown undo event.");
            }

        }

    }
}


