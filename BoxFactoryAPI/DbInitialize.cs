using Core.Services;
using Models.DTOs;
using Models.Util;

namespace BoxFactoryAPI;

public class DbInitialize
{
    private readonly OrderService _orderService;
    private readonly BoxService _boxService;

    public DbInitialize(OrderService orderService, BoxService boxService)
    {
        _orderService = orderService;
        _boxService = boxService;
    }

    public async Task InitializeData()
    {
        var rnd = new Random();
        var numberOfBoxes = rnd.Next(20, 30);
        var materials = new List<string> {"cardboard", "plastic", "wood", "metal"};

        var colors = new List<string> {"red", "blue", "green", "yellow", "black", 
            "white", "brown", "grey", "orange", "purple", 
            "pink", "gold", "silver", "bronze", "copper"};
        Console.WriteLine("Creating boxes...");
        for (int i = 0; i < numberOfBoxes; i++)
        {
            var randomMaterial = materials[rnd.Next(materials.Count)];
            var randomColor = colors[rnd.Next(colors.Count)];

            await _boxService.Create(new BoxCreateDto()
            {
                Colour = randomColor,
                Material = randomMaterial,
                Price = rnd.NextSingle() * 100,
                Stock = rnd.Next(100, 10000),
                DimensionsDto = new DimensionsDto()
                {
                    Height = rnd.Next(1, 100),
                    Length = rnd.Next(1, 100),
                    Width = rnd.Next(1, 100)
                },
                Weight = rnd.NextSingle() * 100
            });
        }
        
        var boxParameter = new BoxParameters()
        {
            BoxesPerPage = 2000,
            CurrentPage = 1,
            SearchTerm = "",
            Descending = false
        };
        var sorting = new Sorting(null, null);
        var boxes = (await _boxService.Get(boxParameter, sorting)).Boxes!.ToList();

        var firstNames = new List<string> {"John", "Jane", "Jim", "Jenny", "James", "Judy", "Joe", "Jessica", "Jack", "Julia"};

        var lastNames = new List<string> {"Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Garcia", "Rodriguez", "Wilson"};
        
        var cities = new List<string> {"New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose"};
        var countries = new List<string> {"USA", "Canada", "Mexico", "UK", "France", "Germany", "Netherlands", "Belgium", "Italy", "Spain"};
        var postalCodes = new List<string> {"10001", "90001", "60601", "77001", "85001", "19101", "78201", "92101", "75201", "95101"};
        var houseNumbers = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
        var houseNumberAdditions = new List<string> {"A", "B", "C", "D", "", "", "", "", "", ""}; // Assuming house number addition can be an empty string
        var date = DateTime.Now;
        Console.WriteLine("Creating orders...");
        var currentMonth = date.Month;
        for (int i = 0; i < currentMonth; i++)
        {
            Console.WriteLine($"{i/currentMonth*100}%");
            date = date.AddMonths(-1);
            var numberOfOrders = rnd.Next(5, 15);
            for (int j = 0; j < numberOfOrders; j++)
            {
                var numberOfBoxesInOrder = rnd.Next(1, 3);
                var saveBoxes = new Dictionary<Guid, int>();
                for (int k = 0; k < numberOfBoxesInOrder; k++)
                {
                    var selectBox = boxes[rnd.Next(boxes.Count)].Id;
                    saveBoxes.TryAdd(selectBox, rnd.Next(1, 10));
                }

                try
                {
                    var createOrder = new OrderCreateDto()
                    {
                        Boxes = saveBoxes,
                        Customer = new CreateCustomerDto()
                        {
                            Address = new CreateAddressDto()
                            {
                                City = cities[rnd.Next(cities.Count)],
                                Country = countries[rnd.Next(countries.Count)],
                                StreetName = lastNames[rnd.Next(lastNames.Count)],
                                PostalCode = postalCodes[rnd.Next(postalCodes.Count)],
                                HouseNumber = houseNumbers[rnd.Next(houseNumbers.Count)],
                                HouseNumberAddition = houseNumberAdditions[rnd.Next(houseNumberAdditions.Count)]
                            },
                            FirstName = firstNames[rnd.Next(firstNames.Count)],
                            LastName = lastNames[rnd.Next(lastNames.Count)],
                            Email =
                                $"{firstNames[rnd.Next(firstNames.Count)]}.{lastNames[rnd.Next(lastNames.Count)]}@gmail.com",
                            PhoneNumber = string.Join("", Enumerable.Range(0, 10).Select(n => rnd.Next(10).ToString()))
                        }
                    };
                    await _orderService.Create(createOrder, date);
                }
                catch (Exception)
                {
                    numberOfBoxes++;
                }
            }
        }
    }
}