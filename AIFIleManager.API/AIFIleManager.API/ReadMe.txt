# 1. Create and enter the project
dotnet new webapi -n AIFileManager.API
cd AIFileManager.API

# 2. Add packages
dotnet add package Newtonsoft.Json
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.Extensions.Http

# 3. Run the API
dotnet run

# 4. (Optional) Create and run FastAPI service
pip install fastapi uvicorn ollama pydantic requests tensorflow numpy
uvicorn main:app --reload
