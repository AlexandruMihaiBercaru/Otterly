add-migration +NAME:
    dotnet ef migrations add {{NAME}} --output-dir="./Data/Migrations"
    dotnet ef database update