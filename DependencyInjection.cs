using TMSWeb.Repositories;
using TMSWeb.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<AuditLogService>();
        services.AddScoped<AgentRepository>();
        services.AddScoped<AllowanceRateRepository>();
        services.AddScoped<ApproveRepository>();
        services.AddScoped<BillingRepository>();
        services.AddScoped<BudgetRepository>();
        services.AddScoped<CodeRepository>();
        services.AddScoped<CoverSheetRepository>();
        services.AddScoped<CustomerRepository>();
        services.AddScoped<DashboardRepository>();
        services.AddScoped<DropListRepository>();
        services.AddScoped<EmployeeRepository>();
        services.AddScoped<ExportRepository>();
        services.AddScoped<FormBillingRepository>();
        services.AddScoped<FormAVARepository>();
        services.AddScoped<FormRepository>();
        services.AddScoped<FuelRateRepository>();
        services.AddScoped<JobRepository>();
        services.AddScoped<LayoutRepository>();
        services.AddScoped<OrderRepository>();
        services.AddScoped<OtherReceiptRepository>();
        services.AddScoped<PayeeRepository>();
        services.AddScoped<PaymentRepository>();
        services.AddScoped<PlaceRepository>();
        services.AddScoped<QRCodeRepository>();
        services.AddScoped<QuotationRepository>();
        services.AddScoped<ReceiptRepository>();
        services.AddScoped<ReportRepository>();
        services.AddScoped<SalaryRepository>();
        services.AddScoped<SearchRepository>();
        services.AddScoped<SupplierRepository>();
        services.AddScoped<TrailerRepository>();
        services.AddScoped<TransportRateRepository>();
        services.AddScoped<TruckRepository>();
        services.AddScoped<VatInvoiceRepository>();
        services.AddScoped<YardPortRepository>();
        services.AddScoped<ZoneRepository>();


        // เพิ่ม Repository อื่นตรงนี้
        // services.AddScoped<CustomerRepository>();
        // services.AddScoped<SupplierRepository>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditLogService, AuditLogService>();

        return services;
    }
}