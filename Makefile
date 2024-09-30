SLN=Altinn.Authorization

dotnet_solution: clean
	dotnet new sln -n $(SLN)
	find src -name "*.csproj" -print0 | xargs -0 dotnet sln add

clean:
	@rm -f $(SLN).sln
	echo "** Clean Solution **"