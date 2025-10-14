import * as signalR from '@microsoft/signalr';

let connection;

export function getChatConnection({ baseUrl, getToken }) {
	if (connection) return connection;

	connection = new signalR.HubConnectionBuilder()
		.withUrl(`${baseUrl}/chatHub`, {
			accessTokenFactory: () => getToken?.() ?? '',
			skipNegotiation: false,
			transport: signalR.HttpTransportType.WebSockets
		})
		.withAutomaticReconnect({
			nextRetryDelayInMilliseconds: ctx => {
				const delays = [0, 1000, 2000, 5000, 10000];
				return delays[Math.min(ctx.previousRetryCount + 1, delays.length - 1)];
			}
		})
		.configureLogging(signalR.LogLevel.Information)
		.build();

	return connection;
}