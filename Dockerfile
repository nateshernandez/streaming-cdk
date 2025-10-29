FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/StreamingApi/ StreamingApi/
RUN dotnet publish StreamingApi/StreamingApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=public.ecr.aws/awsguru/aws-lambda-adapter:0.8.4 /lambda-adapter /opt/extensions/lambda-adapter

COPY --from=build /app/publish .

ENV PORT=8080
ENV AWS_LAMBDA_EXEC_WRAPPER=/opt/bootstrap

EXPOSE 8080
ENTRYPOINT ["dotnet", "StreamingApi.dll"]