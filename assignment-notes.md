CSE 325 WEEK 01 ASSIGNMENT
=============================================================

1. EVIDENCE – CREATE A WEB API WITH ASP.NET CORE CONTROLLERS

I completed the Microsoft Learn "Create a web API with ASP.NET Core controllers" module using the ContosoPizza project.

The API initially contained the following pizzas:

[
  {
    "id": 1,
    "name": "Classic Italian",
    "isGlutenFree": false
  },
  {
    "id": 2,
    "name": "Veggie",
    "isGlutenFree": true
  }
]

I then used the POST endpoint to add an additional pizza named Hawaii:

{
  "id": 3,
  "name": "Hawaii",
  "isGlutenFree": false
}

I later used the PUT endpoint to update the pizza name from Hawaii to Hawaiian:

{
  "id": 3,
  "name": "Hawaiian",
  "isGlutenFree": false
}

After adding and updating the pizza, the API contained the original pizzas and my additional pizza:

[
  {
    "id": 1,
    "name": "Classic Italian",
    "isGlutenFree": false
  },
  {
    "id": 2,
    "name": "Veggie",
    "isGlutenFree": true
  },
  {
    "id": 3,
    "name": "Hawaiian",
    "isGlutenFree": false
  }
]


2. CRUD OPERATIONS


GET – RETRIEVE ALL PIZZAS

Request:

GET https://localhost:7225/pizza
Accept: application/json

Status Code:

200 OK

Response:

[
  {
    "id": 1,
    "name": "Classic Italian",
    "isGlutenFree": false
  },
  {
    "id": 2,
    "name": "Veggie",
    "isGlutenFree": true
  }
]

The GET operation successfully retrieved the pizzas stored by the application.


POST – CREATE A PIZZA

Request:

POST https://localhost:7225/pizza
Content-Type: application/json

{
  "name": "Hawaii",
  "isGlutenFree": false
}

Status Code:

201 Created

Response:

{
  "id": 3,
  "name": "Hawaii",
  "isGlutenFree": false
}

The POST operation successfully created a new pizza and assigned it ID 3.


PUT – UPDATE A PIZZA

Request:

PUT https://localhost:7225/pizza/3
Content-Type: application/json

{
  "id": 3,
  "name": "Hawaiian",
  "isGlutenFree": false
}

Status Code:

204 No Content

Response:

No response body was returned.

I verified the update with:

GET https://localhost:7225/pizza/3
Accept: application/json

Status Code:

200 OK

Response:

{
  "id": 3,
  "name": "Hawaiian",
  "isGlutenFree": false
}

The PUT operation successfully changed the pizza name from Hawaii to Hawaiian.


DELETE – DELETE A PIZZA

Request:

DELETE https://localhost:7225/pizza/3

Status Code:

204 No Content

Response:

No response body was returned.

I verified the deletion with:

GET https://localhost:7225/pizza/3
Accept: application/json

Status Code:

404 Not Found

This confirmed that pizza ID 3 had been successfully deleted.


3. WORKING SALES SUMMARY FUNCTION

The following function was added to my "Work with files and directories in a .NET app" project. It creates a sales summary containing the grand total and the sales total from each JSON sales file.


string GenerateSalesReport(
    IEnumerable<string> salesFiles,
    double salesTotal,
    string storesDirectory)
{
    StringBuilder report = new StringBuilder();

    // Add the report heading
    report.AppendLine("Sales Summary");
    report.AppendLine("----------------------------");

    // Add the grand total
    report.AppendLine(
        $"Total Sales: {salesTotal:C}"
    );

    report.AppendLine();

    // Add the details heading
    report.AppendLine("Details:");

    // Add the sales total from each JSON file
    foreach (var file in salesFiles)
    {
        // Read the JSON file
        string salesJson = File.ReadAllText(file);

        // Convert the JSON into a SalesData object
        SalesData? data =
            JsonConvert.DeserializeObject<SalesData?>(
                salesJson
            );

        if (data != null)
        {
            // Get the file path relative to the stores folder
            string fileName = Path.GetRelativePath(
                storesDirectory,
                file
            );

            // Add the file and its sales total to the report
            report.AppendLine(
                $"{fileName}: {data.Total:C}"
            );
        }
    }

    return report.ToString();
}


The generated sales report is:

Sales Summary
----------------------------
Total Sales: $2,012.20

Details:
sales.json: $88.88
201\sales.json: $501.22
201\salestotals.json: $0.00
202\sales.json: $1,234.22
202\salestotals.json: $0.00
203\sales.json: $99.00
203\salestotals.json: $0.00
204\sales.json: $88.88
204\salestotals.json: $0.00


The program writes the completed report to:
salesTotalDir/totals.txt


