import * as signalR from '@microsoft/signalr';

export function createChatConnection({ baseUrl, getToken }) {
	return new signalR.HubConnectionBuilder()
		.withUrl(`${baseUrl}/chatHub`, {
			accessTokenFactory: () => getToken?.() ?? '',
			skipNegotiation: true,
			transport: signalR.HttpTransportType.WebSockets,
			withCredentials: true
		})
		.withAutomaticReconnect({
			nextRetryDelayInMilliseconds: ctx => {
				const delays = [0, 1000, 2000, 5000, 10000];
				return delays[Math.min(ctx.previousRetryCount + 1, delays.length - 1)];
			}
		})
		.configureLogging(signalR.LogLevel.Information)
		.build();
}