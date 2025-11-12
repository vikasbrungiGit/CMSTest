using CorporateManagementService;
using Microsoft.IdentityModel.Tokens;
using System.ServiceModel;
using System;
using System.Threading.Tasks;
using UhaulComIntegration.Logging;
using Microsoft.Extensions.Logging;

class Program
{
    private static CorporateManagementServiceClient _corporateManagementServiceClient;
    private static IUhaulComLogger _uhaulComLogger; // Ensure this is initialized appropriately

    public Program(IUhaulComLogger logger)
    {
        
        _uhaulComLogger = logger;
    }
    static async Task Main(string[] args)
    {
        _corporateManagementServiceClient = CreateCorporateManagementServiceClient();

        //_uhaulComLogger = new IUhaulComLogger();

        var account = await GetCorporateAccountAsync("99821475");

        var updatedAccount = await SaveCorporateAccountAsync(account);

        Console.WriteLine("Hello, World!");
    }

    private static async Task<SaveAccountResponse?> SaveCorporateAccountAsync(Account account)
    {
        var saveRequest = new SaveAccountRequest
        {
            Account = account,
            AccountUserID = 119834,
            HasBillingUpdates = true,
            SystemSourceTypeCode = SystemSourceTypeCode.UHaulComStore
        };

        saveRequest.Account.PaymentTokens ??= Array.Empty<PaymentToken>();

        //var response = await _uhaulComLogger.TraceServiceCall(
        //                                        serviceName: "CorpServiceTest",
        //                                        serviceMethod: nameof(_corporateManagementServiceClient.SaveAccountAsync),
        //                                        request: saveRequest,
        //                                        codeBlock: _corporateManagementServiceClient.SaveAccountAsync
        //                                    );
        var response = await _corporateManagementServiceClient.SaveAccountAsync(saveRequest);
        return response;
    }

    public static async Task<Account?> GetCorporateAccountAsync(string accountNumber)
    {
        GetCorporateAccountRequest request = new()
        {
            AccountNumber = accountNumber,
            AccountId = null,
            IncludeCustomerEmail = false
        };

        try
        {
            //var response = await _uhaulComLogger.TraceServiceCall(serviceName: "CorpServiceTest",
            //    serviceMethod: nameof(_corporateManagementServiceClient.GetCorporateAccountAsync),
            //    request: request,
            //    codeBlock: _corporateManagementServiceClient.GetCorporateAccountAsync
            //    );
            var response = await _corporateManagementServiceClient.GetCorporateAccountAsync(request);
            return response?.CorporateAccount;
        }
        catch (Exception ex)
        {
            await _uhaulComLogger.ErrorAsync($"{nameof(GetCorporateAccountAsync)}-{ex.Message}", ex.StackTrace, ex: ex);
            return null;
        }
    }

    private static CorporateManagementServiceClient CreateCorporateManagementServiceClient()
    {
        var endpoint = new EndpointAddress("https://uhiservicesd.amerco.org/CorporateManagementService/CorporateManagementService.svc");
        var binding = new WSHttpBinding
        {
            Security = { Mode = SecurityMode.Transport },
            MaxReceivedMessageSize = int.MaxValue,
            CloseTimeout = new TimeSpan(0, 2, 0),
            OpenTimeout = new TimeSpan(0, 2, 0),
            // ReceiveTimeout = new TimeSpan(0, 2, 0), // The default is 10 minutes -- let's keep that.
            SendTimeout = new TimeSpan(0, 2, 0)
        };

        return new CorporateManagementServiceClient(binding, endpoint);
    }
}
