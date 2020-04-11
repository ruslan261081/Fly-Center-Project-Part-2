using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FlyCenterProject;
using System.IO;
using System.Data.SqlClient;
using Newtonsoft.Json;
using static DB_Generator.ApiModel;
using System.Data;

namespace DB_Generator
{
   internal class RandomDataGenerator
   {
        private HttpClient client = new HttpClient();
        private string url = "https://randomuser.me/api";
        public static List<string> countryList;
        private  List<string> AddCountriesToList;
        private Random _random;
        private int _randomNum;
       
       

        private static Customer[] customersArray;
        private static string[] countriesToAddArray;
        private static List<string> airlinesList;
        public static string[] airlineCompaniesArray;
        private string _charasters = "abcdefghijklmnoprst1234567890";

        public string Message { get; set; }
        public int ProgressValue { get; set; }
        public int Total { get; set; }
       
        private CountryDAOMSSQL _countryDAO;
        private CustomerDAOMSSQL _customerDAO;
        private AirlineDAOMSSQL _airlineDAO;
        private FlightDAOMSSQL _flightDAO;
        private TicketDAOMSSQL _ticketDAO;
        private List<Country> _countries;
        private List<AirlineCompany> _airlineCompanies;
        private List<Flight> _flights;
        private List<Ticket> _tickets;
        private List<Customer> _customers;


        public RandomDataGenerator()
        {
            _random = new Random();
            _countryDAO = new CountryDAOMSSQL();
            _customerDAO = new CustomerDAOMSSQL();
            _airlineDAO = new AirlineDAOMSSQL();
            _flightDAO = new FlightDAOMSSQL();
            _ticketDAO = new TicketDAOMSSQL();
            _countries = new List<Country>();
            _customers = new List<Customer>();
            _airlineCompanies = new List<AirlineCompany>();
            _flights = new List<Flight>();
            _tickets = new List<Ticket>();
        }
        internal void AddCustomerToDB(int number)
        {
            int counter = 0;
            try
            {
                List<Customer> customerFromDB = _customerDAO.GetAll().ToList();
                HttpResponseMessage httpResponse = client.GetAsync(url + "?results=" + number).Result;
                for (int i = 0; i < number; i++)
                {
                   // HttpResponseMessage httpResponse = client.GetAsync(url + "?results=" + number).Result;
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        Customer c = new Customer();
                        var content = httpResponse.Content.ReadAsStringAsync().Result;
                        APIUser r = JsonConvert.DeserializeObject<APIUser>(content);
                        if(!customerFromDB.Any(customer => customer.UserName == r.results[i].login.username))
                          
                            c.FirstName = r.results[i].name.first;
                            c.LastName = r.results[i].name.last;
                            c.Password = r.results[i].login.password;
                            c.UserName = r.results[i].login.username;
                            c.PhoneNo = r.results[i].phone;
                            c.Address = r.results[i].location.city;
                            c.CreditCardNumber = r.results[i].cell;
                            _customerDAO.Add(c);
                            _customers.Add(c);
                            counter++;
                        
                        ProgressValue += Convert.ToInt32(1.0 / Total * 100);
                    }

                }
                Message += $"Created Customers {counter}/{number}\n";
            }
            catch(Exception e)
            {
                Message += $"{e.ToString()}\n";
            }
            
        }
        internal void AddCountriesFromApi(int number)
        {
            int counter = 0;
            try
            {
                _countries = _countryDAO.GetAll().ToList();
                HttpResponseMessage httpResponse = client.GetAsync(url + "?results=" + number).Result;

                for (int i = 0; i < number; i++)
                {
                    if(httpResponse.IsSuccessStatusCode)
                    {
                        Country c = new Country();
                        var content = httpResponse.Content.ReadAsStringAsync().Result;
                        APIUser aPIUser = JsonConvert.DeserializeObject<APIUser>(content);


                        if(!_countries.Any(country => country.CountryName == aPIUser.results[i].location.city))
                        {
                            c.CountryName = aPIUser.results[i].location.city;
                            _countryDAO.Add(c);
                            counter++;
                        }
                        ProgressValue += Convert.ToInt32(1.0 / Total * 100);
                    }
                    _countries = _countryDAO.GetAll().ToList();
                    Message = $"Created Countries {counter}/ {number}\n";

                }

            }
            catch(Exception e)
            {
                Message += $"{e.ToString()}\n";
            }
        }
        internal void AddRandomAirlines( int number)
        {
            int counter = 0;
            try
            {
                long LastId = _airlineDAO.GetAll().Any() ? _airlineDAO.GetAll().ToList().Last().ID : 1;
               
                for (int i = 0; i < number; i++)
                {
                    AirlineCompany airline = new AirlineCompany();
                    _randomNum = _random.Next(1, 6);
                    StringBuilder stringBuilder = new StringBuilder(_randomNum);
                    for (int j = 0; j < _randomNum; j++)
                    {
                        stringBuilder.Append(_charasters[_random.Next(_charasters.Length)]);

                    }
                    airline.AirlineName = stringBuilder.ToString() + LastId.ToString();
                    _randomNum = _random.Next(1, 6);
                    for (int j = 0; j < _randomNum; j++)
                    {
                        stringBuilder.Append(_charasters[_random.Next(_charasters.Length)]);
                    }
                    airline.UserName = stringBuilder.ToString();
                    airline.Password = stringBuilder.ToString();
                    _randomNum = _random.Next(0, _countries.Count);
                    airline.CountryCode = _countries[_randomNum].Id;
                    _airlineDAO.Add(airline);
                    _airlineCompanies.Add(airline);
                    counter++;
                    LastId++;
                    ProgressValue += Convert.ToInt32(1.0 / Total * 100);


                }
                Message += $"Created AirlineCompanies {counter}/{number}\n";
            }
            catch(Exception e)
            {
                Message += $"{e.ToString()}\n";
            }
        }
        internal void AddRandomFlights(int number)
        {
            int counter = 0;
            try
            {
                _flights.AddRange(_flightDAO.GetAll().ToList());

                for (int i = 0; i < _airlineCompanies.Count; i++)
                {
                    AirlineCompany c = _airlineDAO.GetAirLineByUserName(_airlineCompanies[i].UserName);
                        Flight f = new Flight();
                        f.Departure_Time = AddRandomDate();
                        f.Landing_Time = AddRandomDate();
                        _randomNum = _random.Next(100,350);
                        f.Remaining_Tickets = _randomNum;
                        _randomNum = _random.Next(0, _countries.Count);
                        f.Destination_Country_Code = _countries[_randomNum].Id;
                        _randomNum = _random.Next(0, _countries.Count);
                        f.Origin_Country_Code = _countries[_randomNum].Id;
                        f.AirlineCompany_Id = c.ID;
                        _flightDAO.Add(f);
                        _flights.Add(f);
                        counter++;
                        ProgressValue += Convert.ToInt32(1.0 / Total * 100);

                    Message += $"Created Flights {counter}/{number} per airline company\n";

                }


            }
            catch (Exception e)
            {
                Message += $"{e.ToString()}\n";
            }
        }
        internal void AddRandomTickets(int number)
        {
            int counter = 0;
            try
            {
              
                for (int i = 0; i < _customers.Count; i++)
                {
                    Customer c = _customerDAO.GetCustomerByUserName(_customers[i].UserName);
                    for (int j = 0; j < number; j++)
                    {
                        Ticket t = new Ticket();
                        t.CustomerID = c.Id;
                        _randomNum = _random.Next(0, _flights.Count);
                        t.FlightID = _flights[_randomNum].ID;
                        if(! _tickets.Any(ticket => ticket.FlightID == t.FlightID && ticket.CustomerID == t.CustomerID))
                        {
                            _ticketDAO.Add(t);
                            _tickets.Add(t);
                            counter++;
                        }
                    }
                    ProgressValue += Convert.ToInt32(1.0 / Total * 100);
                }
                Message += $"Created Airline Tickets {counter}/ {number} per customer\n";

            }
            catch(Exception e)
            {
                Message += $"{e.ToString()}\n";
            }
        }

        public class RootObject
        {
            public int ID { get; set; }
            public string First_Name { get; set; }
            public string Last_Name { get; set; }
            public string User_Name { get; set; }
            public string Password { get; set; }
            public string Address { get; set; }
            public int Phono_Number { get; set; }
            public int Credit_Card_Number { get; set; }
        }
        private DateTime AddRandomDate()
        {
            return DateTime.Now.AddDays(_random.Next(1000));
        }
        internal void DeleteAll()
        {
           

            Message += "Delete the DataBase";
            DeleteDataDAO deleteDataDAO = new DeleteDataDAO();
            deleteDataDAO.CleanAllDatabase();
            
        }
    }
}
