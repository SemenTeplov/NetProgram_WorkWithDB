using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Data.SqlClient;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;

public class Categories
{
    public string Name;
}
public class Cities
{
    public string Name;
}
public class Clients
{
    public string FullName;
    public string DateOfBith;
    public string Gender;
    public string Email;
    public int CountryId;
    public int CityId;
}
public class Countries
{
    public string Name;
}
public class InterestedBuyers
{
    public int ClientId;
    public int CategoryId;
}
public class Products
{
    public string Name;
    public int CategoryId;
}
public class Promotions
{
    public int Percent;
    public string StartDate;
    public string EndDate;
    public int CountryId;
    public int ProducId;
}

class Repository
{
    readonly string connectionString =
        @"Data Source=DESKTOP-VKP7RF3\SQLEXPRESS01;Initial Catalog=MailingsDb;Integrated Security=True";

    public List<Clients> GetClients()
    {
        using var db = new SqlConnection(connectionString);
        db.Open();

        var query = "SELECT Clients.FullName FROM Clients";
        var columns = db.Query<Clients>(query);

        return columns.ToList();
    }
    public List<Clients> GetClientsEmail()
    {
        using var db = new SqlConnection(connectionString);
        db.Open();

        var query = "SELECT Clients.Email FROM Clients";
        var columns = db.Query<Clients>(query);

        return columns.ToList();
    }
    public List<Categories> GetCategories()
    {
        using var db = new SqlConnection(connectionString);
        db.Open();

        var query = "SELECT Categories.Name FROM Categories";
        var columns = db.Query<Categories>(query);

        return columns.ToList();
    }
    public List<Products> GetProducts()
    {
        using var db = new SqlConnection(connectionString);
        db.Open();

        var query = "SELECT Products.Name FROM Products";
        var columns = db.Query<Products>(query);

        return columns.ToList();
    }
    public List<Cities> GetCities()
    {
        using var db = new SqlConnection(connectionString);
        db.Open();

        var query = "SELECT Cities.Name FROM Cities";
        var columns = db.Query<Cities>(query);

        return columns.ToList();
    }
    public List<Countries> GetCountries()
    {
        using var db = new SqlConnection(connectionString);
        db.Open();

        var query = "SELECT Countries.Name FROM Countries";
        var columns = db.Query<Countries>(query);

        return columns.ToList();
    }
    public List<Clients> GetClientsFromCities(string city)
    {
        using var db = new SqlConnection(connectionString);
        db.Open();

        var query = $"SELECT Clients.FullName FROM Clients, Cities WHERE Clients.CityId = Cities.Id AND Cities.Name = '{city}'";
        var columns = db.Query<Clients>(query);

        return columns.ToList();
    }
    public List<Clients> GetClientsFromCountries(string country)
    {
        using var db = new SqlConnection(connectionString);
        db.Open();

        var query = $"SELECT Clients.FullName FROM Clients, Countries WHERE Clients.CountryId = Countries.Id AND Countries.Name = '{country}'";
        var columns = db.Query<Clients>(query);

        return columns.ToList();
    }
    public List<Promotions> GetPromotionsForCountries(string country)
    {
        using var db = new SqlConnection(connectionString);
        db.Open();

        var query = $"SELECT * FROM Promotions, Countries WHERE Promotions.CountryId = Countries.Id AND Countries.Name = '{country}'";
        var columns = db.Query<Promotions>(query);

        return columns.ToList();
    }
    public List<Cities> GetCitiesFromCountries(string country)
    {
        using var db = new SqlConnection(connectionString);
        db.Open();

        var query = $"SELECT Cities.Name FROM Cities, Countries, Clients WHERE Clients.CountryId = Countries.Id AND Clients.CityId = Cities.Id AND Countries.Name = '{country}' GROUP BY Cities.Name";
        var columns = db.Query<Cities>(query);

        return columns.ToList();
    }
    public List<Categories> GetCategoriesOfClients(string client)
    {
        using var db = new SqlConnection(connectionString);
        db.Open();

        var query = $"SELECT Categories.Name FROM Categories, Countries, Clients, Products, Promotions WHERE Clients.CountryId = Countries.Id AND Promotions.CountryId = Countries.Id AND Promotions.ProducId = Products.Id AND Products.CategoryId = Categories.Id AND Clients.FullName = '{client}' GROUP BY Categories.Name";
        var columns = db.Query<Categories>(query);

        return columns.ToList();
    }
    public List<Products> GetProductsForCategories(string category)
    {
        using var db = new SqlConnection(connectionString);
        db.Open();

        var query = $"SELECT Products.Name FROM Products, Categories WHERE Products.CategoryId = Categories.Id AND Categories.Name = '{category}'";
        var columns = db.Query<Products>(query);

        return columns.ToList();
    }
}

public class Programm
{
    private static Repository repository;

    public static void Main()
    {
        repository = new Repository();
        StartServer();
        Console.Read();
    }

    static async Task StartServer()
    {
        HttpListener server = new HttpListener();
        server.Prefixes.Add("http://127.0.0.1:8080/");
        server.Start();

        Console.WriteLine("Server started ...");

        while (true)
        {
            var context = await server.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            using var output = new StreamWriter(context.Response.OutputStream);

            if (request.RawUrl == "/allClients")
            {
                foreach (var item in repository.GetClients())
                {
                    await output.WriteLineAsync(item.FullName);
                }
            }
            else if (request.RawUrl == "/clientsEmail")
            {
                foreach (var item in repository.GetClientsEmail())
                {
                    await output.WriteLineAsync(item.Email);
                }
            }
            else if (request.RawUrl == "/allCategories")
            {
                foreach (var item in repository.GetCategories())
                {
                    await output.WriteLineAsync(item.Name);
                }
            }
            else if (request.RawUrl == "/allProducts")
            {
                foreach (var item in repository.GetProducts())
                {
                    await output.WriteLineAsync(item.Name);
                }
            }
            else if (request.RawUrl == "/allCities")
            {
                foreach (var item in repository.GetCities())
                {
                    await output.WriteLineAsync(item.Name);
                }
            }
            else if (request.RawUrl == "/allCountries")
            {
                foreach (var item in repository.GetCountries())
                {
                    await output.WriteLineAsync(item.Name);
                }
            }
            else if ((request.QueryString.GetValues("city") != null))
            {
                if (request.RawUrl.StartsWith("/clientsFromCities"))
                {
                    foreach (var item in repository.GetClientsFromCities(request.QueryString.GetValues("city")[0].ToString()))
                    {
                        await output.WriteLineAsync(item.FullName);
                    }
                }
            }
            else if ((request.QueryString.GetValues("country") != null))
            {
                if (request.RawUrl.StartsWith("/clientsFromCountries"))
                {
                    foreach (var item in repository.GetClientsFromCountries(request.QueryString.GetValues("country")[0].ToString()))
                    {
                        await output.WriteLineAsync(item.FullName);
                    }
                }
                else if (request.RawUrl.StartsWith("/promotionsForCountries"))
                {
                    foreach (var item in repository.GetPromotionsForCountries(request.QueryString.GetValues("country")[0].ToString()))
                    {
                        await output.WriteLineAsync(item.Percent.ToString() + " "
                            + item.StartDate.ToString() + " "
                            + item.EndDate.ToString());
                    }
                }
                else if (request.RawUrl.StartsWith("/CitiesFromCountries"))
                {
                    foreach (var item in repository.GetCitiesFromCountries(request.QueryString.GetValues("country")[0].ToString()))
                    {
                        await output.WriteLineAsync(item.Name);
                    }
                }
            }
            else if ((request.QueryString.GetValues("client") != null))
            {
                if (request.RawUrl.StartsWith("/categoriesOfClients"))
                {
                    foreach (var item in repository.GetCategoriesOfClients(request.QueryString.GetValues("client")[0].ToString()))
                    {
                        await output.WriteLineAsync(item.Name);
                    }
                }
            }
            else if ((request.QueryString.GetValues("category") != null))
            {
                if (request.RawUrl.StartsWith("/productsForCategories"))
                {
                    foreach (var item in repository.GetProductsForCategories(request.QueryString.GetValues("category")[0].ToString()))
                    {
                        await output.WriteLineAsync(item.Name);
                    }
                }
            }
        }

        server.Stop();
    }
}

