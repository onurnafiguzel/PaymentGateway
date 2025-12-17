# =============================
# BUILD STAGE
# =============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ---- CENTRAL CONFIG FILES (CRITICAL) ----
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

# ---- SOLUTION MODEL (SLNX) ----
COPY PaymentGateway.slnx ./

# ---- SHARED PROJECTS (MUST EXIST FOR RESTORE) ----
COPY src/shared/Shared.Contracts/Shared.Contracts.csproj src/shared/Shared.Contracts/
COPY src/shared/Shared.Kernel/Shared.Kernel.csproj src/shared/Shared.Kernel/
COPY src/shared/Shared.Messaging/Shared.Messaging.csproj src/shared/Shared.Messaging/

# ---- PAYMENT ORCHESTRATOR PROJECTS ----
COPY src/services/payment-orchestrator/PaymentOrchestrator.Domain/PaymentOrchestrator.Domain.csproj src/services/payment-orchestrator/PaymentOrchestrator.Domain/
COPY src/services/payment-orchestrator/PaymentOrchestrator.Application/PaymentOrchestrator.Application.csproj src/services/payment-orchestrator/PaymentOrchestrator.Application/
COPY src/services/payment-orchestrator/PaymentOrchestrator.Infrastructure/PaymentOrchestrator.Infrastructure.csproj src/services/payment-orchestrator/PaymentOrchestrator.Infrastructure/
COPY src/services/payment-orchestrator/PaymentOrchestrator.Api/PaymentOrchestrator.Api.csproj src/services/payment-orchestrator/PaymentOrchestrator.Api/

# ---- RESTORE (NOW IT CAN SEE EVERYTHING) ----
RUN dotnet restore src/services/payment-orchestrator/PaymentOrchestrator.Api/PaymentOrchestrator.Api.csproj

# ---- COPY FULL SOURCE ----
COPY . .

# ---- PUBLISH ----
RUN dotnet publish src/services/payment-orchestrator/PaymentOrchestrator.Api/PaymentOrchestrator.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# =============================
# RUNTIME STAGE
# =============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PaymentOrchestrator.Api.dll"]
