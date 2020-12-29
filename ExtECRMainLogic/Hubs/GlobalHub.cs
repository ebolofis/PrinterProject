using ExtECRMainLogic.Enumerators.ExtECR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExtECRMainLogic.Hubs
{
    public class SignalR_CommsClass
    {
        #region Properties
        /// <summary>
        /// Server hub url
        /// </summary>
        private string connectionUrl;
        /// <summary>
        /// Server hub name
        /// </summary>
        private string connectionHub;
        /// <summary>
        /// Store id used as part of connection name
        /// </summary>
        private string connectionStoreId;
        /// <summary>
        /// Name used as part of connection name
        /// </summary>
        private string connectionName;
        /// <summary>
        /// Force signalR to disconnect
        /// </summary>
        private bool forceStop;
        /// <summary>
        /// Force IamAlive message to invoke
        /// </summary>
        private bool iAmAliveRunning;
        /// <summary>
        /// Hub connection
        /// </summary>
        HubConnection hubConnection;
        #endregion
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<SignalR_CommsClass> logger;
        /// <summary>
        /// Local hub invoker
        /// </summary>
        private readonly LocalHubInvoker localHubInvoker;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionUrl"></param>
        /// <param name="connectionHub"></param>
        /// <param name="connectionStoreId"></param>
        /// <param name="connectionName"></param>
        /// <param name="applicationBuilder"></param>
        public SignalR_CommsClass(string connectionUrl, string connectionHub, string connectionStoreId, string connectionName, IApplicationBuilder applicationBuilder)
        {
            this.connectionUrl = connectionUrl;
            this.connectionHub = connectionHub;
            this.connectionStoreId = connectionStoreId;
            this.connectionName = connectionName;
            this.forceStop = false;
            this.iAmAliveRunning = false;
            this.logger = (ILogger<SignalR_CommsClass>)applicationBuilder.ApplicationServices.GetService(typeof(ILogger<SignalR_CommsClass>));
            this.localHubInvoker = (LocalHubInvoker)applicationBuilder.ApplicationServices.GetService(typeof(LocalHubInvoker));
        }

        #region Public Methods

        /// <summary>
        /// Initialize hub
        /// </summary>
        /// <param name="OnHub_NewReceipt"></param>
        /// <param name="OnHub_NewTableReservation"></param>
        /// <param name="OnHub_PrintItem"></param>
        /// <param name="OnHub_PartialPrintConnectivity"></param>
        /// <param name="OnHub_ConnectedUsers"></param>
        /// <param name="OnHub_IssueReportZ"></param>
        /// <param name="OnHub_IssueReportX"></param>
        /// <param name="OnHub_IssueReport"></param>
        /// <param name="OnHub_CreditCardAmount"></param>
        /// <param name="OnHub_Drawer"></param>
        /// <param name="OnHub_Image"></param>
        /// <param name="OnHub_Kitchen"></param>
        /// <param name="OnHub_KitchenInstruction"></param>
        /// <param name="OnHub_KitchenInstructionLogger"></param>
        /// <param name="OnHub_LcdMessage"></param>
        /// <param name="OnHub_StartWeighting"></param>
        /// <param name="OnHub_StopWeighting"></param>
        /// <param name="OnHub_HeartBeat"></param>
        public async void InitializeHubAsync(Action<string, string, bool, bool, PrintType, string, bool> OnHub_NewReceipt, Action<long> OnHub_NewTableReservation, Action<string, string, PrintType, string, bool> OnHub_PrintItem, Action<string, string, bool> OnHub_PartialPrintConnectivity, Action<string> OnHub_ConnectedUsers, Action<string, string> OnHub_IssueReportZ, Action<string, string> OnHub_IssueReportX, Action<string, string> OnHub_IssueReport, Action<string, string, string> OnHub_CreditCardAmount, Action<string> OnHub_Drawer, Action<string, string, string> OnHub_Image, Action<string, string, string> OnHub_Kitchen, Action<string, string, string> OnHub_KitchenInstruction, Action<string, string, string, string> OnHub_KitchenInstructionLogger, Action<string, string> OnHub_LcdMessage, Action<string, string> OnHub_StartWeighting, Action<string, string> OnHub_StopWeighting, Action OnHub_HeartBeat)
        {
            Disconect();

            string uri = connectionUrl + connectionHub;
            logger.LogInformation($"Setting up SignalR for uri '{uri}'");
            try
            {
                hubConnection = new HubConnectionBuilder()
                    .WithUrl(uri, options =>
                    {
                        options.Headers["name"] = connectionStoreId + "|" + connectionName;
                        options.CloseTimeout = new TimeSpan(0, 0, 20);
                    })
                    .Build();
            }
            catch (Exception exception)
            {
                logger.LogError("Error building hub connection: " + exception.ToString());
                return;
            }

            // Closed Event
            hubConnection.Closed += async (error) =>
            {
                logger.LogError("SignalR connection closed: " + error?.Message);
                localHubInvoker.UpdateGlobalHubConnectionStatus(connectionStoreId + "|" + connectionName, hubConnection.State);
                await Task.Delay(new Random().Next(0, 2) * 1000);
                await Task.Run(async () => await ConnectWithRetryAsync(hubConnection));
            };

            // Reconnecting Event
            hubConnection.Reconnecting += error =>
            {
                logger.LogWarning("SignalR reconnecting: " + error);
                localHubInvoker.UpdateGlobalHubConnectionStatus(connectionStoreId + "|" + connectionName, hubConnection.State);
                return Task.CompletedTask;
            };

            // Reconnected Event
            hubConnection.Reconnected += connectionId =>
            {
                logger.LogInformation("SignalR reconnected to signalR hub with id: " + connectionId);
                localHubInvoker.UpdateGlobalHubConnectionStatus(connectionStoreId + "|" + connectionName, hubConnection.State);
                return Task.CompletedTask;
            };

            logger.LogInformation("Initializing SignalR_CommsClass...");

            // Bind messages and functions
            hubConnection.On("newReceipt", OnHub_NewReceipt);
            hubConnection.On("newTableReservation", OnHub_NewTableReservation);
            hubConnection.On("PrintItem", OnHub_PrintItem);
            hubConnection.On("partialPrintConnectivity", OnHub_PartialPrintConnectivity);
            hubConnection.On("connectedUsers", OnHub_ConnectedUsers);
            hubConnection.On("ZReport", OnHub_IssueReportZ);
            hubConnection.On("XReport", OnHub_IssueReportX);
            hubConnection.On("Report", OnHub_IssueReport);
            hubConnection.On("creditCardAmount", OnHub_CreditCardAmount);
            hubConnection.On("Drawer", OnHub_Drawer);
            hubConnection.On("Image", OnHub_Image);
            hubConnection.On("kitchen", OnHub_Kitchen);
            hubConnection.On("kitchenInstruction", OnHub_KitchenInstruction);
            hubConnection.On("kitchenInstructionLogger", OnHub_KitchenInstructionLogger);
            hubConnection.On("LcdMessage", OnHub_LcdMessage);
            hubConnection.On("StartWeighting", OnHub_StartWeighting);
            hubConnection.On("StopWeighting", OnHub_StopWeighting);
            hubConnection.On("heartbeat", OnHub_HeartBeat);

            Task.Delay(2000).Wait();
            forceStop = false;
            try
            {
                await Task.Run(async () => await ConnectWithRetryAsync(hubConnection));
            }
            catch (Exception exception)
            {
                logger.LogError("Error connecting to signalR hub: " + exception.ToString());
            }
        }

        /// <summary>
        /// Terminate hub
        /// </summary>
        public void TerminateHub()
        {
            Disconect();
        }

        /// <summary>
        /// Reset hub
        /// </summary>
        public void ResetConnection()
        {
            if (hubConnection != null)
            {
                hubConnection.StopAsync().Wait();
            }
        }

        /// <summary>
        /// Get general connection status
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            bool isConnected;
            if (hubConnection.State == HubConnectionState.Connected)
                isConnected = true;
            else
                isConnected = false;
            return isConnected;
        }

        #endregion

        #region Invoke Methods

        /// <summary>
        /// Send ExtECR errors to POS.
        /// </summary>
        /// <param name="errorMessage"></param>
        public void Invoke_ExtECRError(string errorMessage)
        {
            if (hubConnection == null)
            {
                logger.LogInformation("NULL PROXY @ Invoke_ExtECRError");
                return;
            }
            try
            {
                hubConnection.InvokeAsync("extecrError", connectionStoreId + '|' + connectionName, errorMessage);
            }
            catch (Exception exception)
            {
                logger.LogError("at Invoke_ExtECRError, " + exception.ToString());
            }
        }

        /// <summary>
        /// Send partial print response message to POS.
        /// </summary>
        /// <param name="posName"></param>
        /// <param name="extecrName"></param>
        /// <param name="resetOrder"></param>
        public void Invoke_PartialPrintConnectivity(string posName, string extecrName, bool resetOrder)
        {
            if (hubConnection == null)
            {
                logger.LogInformation("NULL PROXY @ Invoke_PartialPrintConnectivity");
                return;
            }
            try
            {
                hubConnection.InvokeAsync("partialPrintConnectivity", new object[] { posName, extecrName, resetOrder });
            }
            catch (Exception exception)
            {
                logger.LogError("at Invoke_PartialPrintConnectivity, " + exception.ToString());
            }
        }

        /// <summary>
        /// Send Z report response to POS.
        /// </summary>
        /// <param name="zResponseMessage"></param>
        public void Invoke_ZReportResponse(string zResponseMessage)
        {
            if (hubConnection == null)
            {
                logger.LogInformation("NULL PROXY @ Invoke_ZReportResponse");
                return;
            }
            try
            {
                hubConnection.InvokeAsync("zReportResponse", connectionStoreId + '|' + connectionName, zResponseMessage);
            }
            catch (Exception exception)
            {
                logger.LogError("at Invoke_ZReportResponse, " + exception.ToString());
            }
        }

        /// <summary>
        /// Send weighed quantity to POS.
        /// </summary>
        /// <param name="posName"></param>
        /// <param name="weight"></param>
        public void Invoke_ItemWeighted(string posName, string weight)
        {
            if (hubConnection == null)
            {
                logger.LogInformation("NULL PROXY @ Invoke_ItemWeighted");
                return;
            }
            try
            {
                hubConnection.InvokeAsync("itemWeighted", posName, weight);
            }
            catch (Exception exception)
            {
                logger.LogError("at Invoke_ItemWeighted, " + exception.ToString());
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Disconnect signalR
        /// </summary>
        private void Disconect()
        {
            forceStop = true;
            if (hubConnection != null)
            {
                hubConnection.StopAsync().Wait();
                hubConnection.DisposeAsync().Wait();
                hubConnection = null;
            }
        }

        /// <summary>
        /// Connect signalR
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private async Task<bool> ConnectWithRetryAsync(HubConnection connection)
        {
            while (!forceStop && connection != null)
            {
                try
                {
                    await connection.StartAsync();
                    logger.LogInformation("SignalR connection started");
                    await connection.InvokeAsync("AddToGroup", "ExtECR");
                    if (!iAmAliveRunning)
                    {
                        Thread thread = new Thread(IAmAlive);
                        thread.IsBackground = true;
                        thread.Start();
                    }
                    return true;
                }
                catch (Exception exception)
                {
                    logger.LogError("SignalR connection not started: " + exception.Message);
                    await Task.Delay(2000);
                }
                finally
                {
                    do
                    {
                        localHubInvoker.UpdateGlobalHubConnectionStatus(connectionStoreId + "|" + connectionName, hubConnection.State);
                        await Task.Delay(500);
                    } while (hubConnection.State == HubConnectionState.Connecting);
                    localHubInvoker.UpdateGlobalHubConnectionStatus(connectionStoreId + "|" + connectionName, hubConnection.State);
                }
            }
            return true;
        }

        /// <summary>
        /// Invoke IamAlive message
        /// </summary>
        private void IAmAlive()
        {
            while (true)
            {
                iAmAliveRunning = true;
                Thread.Sleep(5 * 60 * 1000);
                try
                {
                    hubConnection.InvokeAsync("IamAlive").Wait();
                }
                catch (Exception exception)
                {
                    logger.LogError("Error invoking IamAlive message: " + exception.ToString());
                    iAmAliveRunning = false;
                    return;
                }
            }
        }

        #endregion

    }
}