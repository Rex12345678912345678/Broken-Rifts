using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ABH.Shared.DTOs;
using ABH.Shared.Generic;
using Chimera.Library.Components.Interfaces;
using Chimera.Library.Components.Services;
using UnityEngine;

namespace Assets.Scripts.Services.Logic
{
	public class ChimeraBackendService
	{
		private string m_serverUrlTemplate = "https://{environment}.abh.chimeraws.com/api/{api_version}/";

		private string m_apiVersion;

		private string m_serverEnvironment;

		private ISerializer m_serializer;

		private string m_authToken;
		
		private string m_decryptedRovioId;

		private bool m_initialized;

		private List<ServerCallQueueInfo> m_serverCallQueue;

		private string m_cachedServerBaseUrl;

		private string m_serverBaseUrl
		{
			get
			{
				if (!string.IsNullOrEmpty(m_cachedServerBaseUrl))
				{
					return m_cachedServerBaseUrl;
				}
				var newValue = m_serverEnvironment;
				if (m_serverEnvironment.ToLower().Contains("live"))
				{
					newValue = m_serverEnvironment + "-" + m_apiVersion.Replace('.', '-');
				}
				m_cachedServerBaseUrl = m_serverUrlTemplate.Replace("{environment}", newValue).Replace("{api_version}", m_apiVersion);
				return m_cachedServerBaseUrl;
			}
		}

		public string AuthToken
		{
			get
			{
				return m_authToken;
			}
		}
		
		public string RovioId
		{
			get
			{
				return m_decryptedRovioId;
			}
		}

		private string GetServiceRequestEndpoint(BaseRequestDto request)
		{
			DebugLog.Log(GetType(), "GetServiceRequestEndpoint for : " + request.GetType());
			var stringBuilder = new StringBuilder();
			if (request.GetType() == typeof(GetEventLeaderboardRequestDto))
			{
				return "event/leaderboard";
			}
			if (request.GetType() == typeof(GetBossDefeatLogRequestDto))
			{
				return "event/boss";
			}
			if (request.GetType() == typeof(AuthRequestDto))
			{
				return "auth";
			}
			if (request.GetType() == typeof(AddEventScoreRequestDto))
			{
				return "event/score";
			}
			if (request.GetType() == typeof(TrackBossDefeatRequestDto))
			{
				return "event/boss";
			}
			if (request.GetType() == typeof(AddPvpScoreRequestDto))
			{
				return "pvp/score";
			}
			if (request.GetType() == typeof(GetPvpLeaderboardRequestDto))
			{
				return "pvp/leaderboard";
			}
			return "status/";
		}

		public bool Init()
		{
			DebugLog.Log(GetType(), "Init: reading api version file and setting endpoint...");
			var textAsset = Resources.Load("server_api_version") as TextAsset;
			if (textAsset != null)
			{
				m_apiVersion = textAsset.text.Trim().TrimEnd('\r', '\n');
				m_serverEnvironment = "live";
				if (m_serverEnvironment == "dev" || m_serverEnvironment == "test")
				{
					m_serializer = new StringSerializerNewtonSoftImpl();
				}
				else
				{
					m_serializer = DIContainerInfrastructure.GetBinarySerializer();
				}
				m_serverCallQueue = new List<ServerCallQueueInfo>();
				m_initialized = true;
				return true;
			}
			DebugLog.Error(GetType(), "Init: server_api_version.txt file not found!");
			return false;
		}

		public IEnumerator SendRequest<TResponse>(BaseRequestDto dto, HttpMethods method, Action<TResponse> successHandler = null, Action<int> errorHandler = null) where TResponse : BaseResponseDto
		{
			DebugLog.Log(GetType(), string.Concat("SendRequest: ", method, " ", dto.GetType()));
			if (!m_initialized)
			{
				DebugLog.Error(GetType(), "SendRequest: Not yet initialized!");
			}
			else
			{
				if (m_serializer == null)
				{
					yield break;
				}
				var serviceRequestUrl = new StringBuilder(m_serverBaseUrl).Append(GetServiceRequestEndpoint(dto));
				byte[] postArg = null;
				switch (method)
				{
				case HttpMethods.POST:
					try
					{
						if (m_serverEnvironment == "dev" || m_serverEnvironment == "test")
						{
							postArg = Encoding.ASCII.GetBytes(m_serializer.Serialize(dto).ToCharArray());
						}
						else
						{
							postArg = m_serializer.SerializeToBytes(dto);
						}
					}
					catch (Exception ex)
					{
						var e = ex;
						DebugLog.Error(e);
						yield break;
					}
					break;
				case HttpMethods.GET:
					AppendGetParameters(dto, ref serviceRequestUrl);
					break;
				}
				var headers = GetHeaders();
				if (string.IsNullOrEmpty(m_authToken) && dto.GetType() != typeof(AuthRequestDto))
				{
					DebugLog.Warn(GetType(), "GetHeaders: No auth token for backend found!! Need to authenticate user first!");
					// there will NEVER be an auth token offline, just call the error handler
					if (errorHandler != null)
						errorHandler(0);
					yield break;
				}
				if (headers != null)
				{
					var swatch = new Stopwatch();
					swatch.Start();
					using (var www = new WWW(serviceRequestUrl.ToString(), postArg, headers))
					{
						yield return www;
						swatch.Stop();
						var responseTimeParams = new Dictionary<string, string>
						{
							{
								"ResponseTime",
								swatch.ElapsedMilliseconds.ToString()
							},
							{
								"ServiceEndpoint",
								serviceRequestUrl.ToString()
							}
						};
						DIContainerInfrastructure.GetAnalyticsSystem(true).LogEventWithParameters(ABHAnalyticsEvents.ServerRequest, responseTimeParams);
						if (!string.IsNullOrEmpty(www.error))
						{
							DebugLog.Error(GetType(), www.error);
							if (errorHandler != null)
							{
								errorHandler(1);
							}
						}
						else
						{
							DeserializeResponse(successHandler, errorHandler, www);
						}
						yield break;
					}
				}
				DebugLog.Error(GetType(), "SendRequest: Could not create request headers!");
			}
		}

		private void AppendGetParameters(BaseRequestDto dto, ref StringBuilder serviceRequestUrl)
		{
			if (serviceRequestUrl == null)
			{
				return;
			}
			var type = dto.GetType();
			serviceRequestUrl.Append("?");
			var properties = type.GetProperties();
			foreach (var propertyInfo in properties)
			{
				if (propertyInfo.CanRead)
				{
					if (propertyInfo.GetIndexParameters().Length == 0)
					{
						var text = WWW.EscapeURL(propertyInfo.Name);
						var text2 = WWW.EscapeURL(propertyInfo.GetValue(dto, null) as string);
						serviceRequestUrl.Append(text + "=" + text2 + "&");
					}
					else
					{
						DebugLog.Error("INDEXED TYPE: " + propertyInfo.Name + " not added to GET parameters.");
					}
				}
			}
		}

		private void DeserializeResponse<TResponse>(Action<TResponse> successHandler, Action<int> errorHandler, WWW response) where TResponse : BaseResponseDto
		{
			var val = (TResponse)null;
			try
			{
				val = m_serializer.Deserialize<TResponse>(response.bytes);
			}
			catch (Exception ex)
			{
				var stringBuilder = new StringBuilder(GetType().ToString());
				stringBuilder.Append(" SendRequest: Failed to deserialize content! ");
				stringBuilder.AppendLine(string.Concat(ex.GetType(), " ", ex.Message));
				DebugLog.Error(stringBuilder);
			}
			if (val != null)
			{
				DebugLog.Log(GetType(), "SendRequest: Server Response acquired: " + val.Result);
				if (val.Result == RESTResultEnum.Success && successHandler != null)
				{
					successHandler(val);
					return;
				}
				ServerFailHandler(val.Result);
				if (errorHandler != null)
				{
					errorHandler((int)val.Result);
				}
			}
			else
			{
				DebugLog.Warn(GetType(), "SendRequest: Empty Response from server!");
			}
		}

		private void ServerFailHandler(RESTResultEnum responseResult)
		{
			var rESTResultEnum = responseResult;
			if (rESTResultEnum != RESTResultEnum.Success)
			{
				DebugLog.Error(GetType(), "ServerFailHandler: " + responseResult);
			}
		}

		private void StartAsynchPostCall<TRequest, TResponse>(TRequest request, Action<TResponse> onSuccess, Action<int> onError) where TRequest : BaseRequestDto where TResponse : BaseResponseDto
		{
			DebugLog.Log(GetType(), "StartAsynchPostCall");
			var coreStateMgr = DIContainerInfrastructure.GetCoreStateMgr();
			coreStateMgr.StartCoroutine(SendRequest(request, HttpMethods.POST, onSuccess, onError));
		}

		private void StartAsynchGetCall<TRequest, TResponse>(TRequest request, Action<TResponse> onSuccess, Action<int> onError) where TRequest : BaseRequestDto where TResponse : BaseResponseDto
		{
			DebugLog.Log(GetType(), "StartAsynchGetCall");
			var coreStateMgr = DIContainerInfrastructure.GetCoreStateMgr();
			coreStateMgr.StartCoroutine(SendRequest(request, HttpMethods.GET, onSuccess, onError));
		}

		private TRequest CreateDto<TRequest>(TRequest dto) where TRequest : BaseRequestDto
		{
			dto.v = m_apiVersion;
			dto.ClientVersion = DIContainerInfrastructure.GetVersionService().StoreVersion;
			return dto;
		}

		private Dictionary<string, string> GetHeaders()
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.Add("Content-Type", "application/x-protobuf");
			dictionary.Add("PlayerId", DIContainerInfrastructure.IdentityService.SharedId);
			// dictionary.Add("RovioAccessToken", ContentLoader.Instance.m_BeaconConnectionMgr.Identity.GetAccessToken());
			dictionary.Add("RovioAccessToken", "access-token");
			dictionary.Add("AuthToken", m_authToken ?? string.Empty);
			return dictionary;
		}

		public void Authenticate(Action<AuthResponseDto> onSuccess, Action<int> onError)
		{
			var request = CreateDto(new AuthRequestDto());
			StartAsynchGetCall(request, delegate(AuthResponseDto response)
			{
				OnAuthSuccess(response);
				if (onSuccess != null)
				{
					onSuccess(response);
				}
			}, onError);
		}

		private void OnAuthSuccess(AuthResponseDto response)
		{
			foreach (var item in m_serverCallQueue)
			{
				DebugLog.Log(GetType(), "OnAuthSuccess: Found queued server request for " + item.requestDto.GetType());
			}
			DIContainerInfrastructure.IdentityService.OnLoggedIn -= InvalidateAndRefreshAuth;
			DIContainerInfrastructure.IdentityService.OnLoggedIn += InvalidateAndRefreshAuth;
			m_authToken = response.PlayerToken;
			m_decryptedRovioId = response.UnencryptedRovioId;
		}

		private void InvalidateAndRefreshAuth()
		{
			m_authToken = string.Empty;
			Authenticate(null, null);
		}

		public void AddEventScore(string boardName, long score, Action onSuccess, Action<int> onError)
		{
			var request = CreateDto(new AddEventScoreRequestDto
			{
				Score = (int)score
			});
			StartAsynchPostCall<AddEventScoreRequestDto, AddEventScoreResponseDto>(request, delegate
			{
				onSuccess();
			}, onError);
		}

		public void AddEventScore(string boardName, int score, int luckyCoins, int matchmakingScore, GameplayEventType eventType, uint eventEndTime, ScoreSourceType source, Action<AddEventScoreResponseDto> onSuccess, Action<int> onError)
		{
			var request = CreateDto(new AddEventScoreRequestDto
			{
				Score = score,
				GameplayEventType = eventType,
				EventId = boardName,
				MatchMakingScore = matchmakingScore,
				ScoreType = source,
				LuckyCoins = luckyCoins
			});
			StartAsynchPostCall(request, onSuccess, onError);
		}

		public void GetBossDefeatLog(string leaderboardId, Action<GetBossDefeatLogResponseDto> onSuccess, Action<int> onError)
		{
			var request = CreateDto(new GetBossDefeatLogRequestDto
			{
				EventLeaderboardId = leaderboardId
			});
			StartAsynchGetCall(request, onSuccess, onError);
		}

		public void GetEventLeaderboard(string leaderboardId, Action<GetLeaderboardResponseDto> onSuccess, Action<int> onError)
		{
			var request = CreateDto(new GetEventLeaderboardRequestDto
			{
				LeaderboardId = leaderboardId
			});
			StartAsynchGetCall(request, onSuccess, onError);
		}

		public void GetPvpLeaderboard(string leaderboardId, Action<GetLeaderboardResponseDto> onSuccess, Action<int> onError)
		{
			var request = CreateDto(new GetPvpLeaderboardRequestDto
			{
				LeaderboardId = leaderboardId
			});
			StartAsynchGetCall(request, onSuccess, onError);
		}

		public void AddPvpScore(string seasonName, int turn, PvpLeague league, int score, int luckyCoins, int matchmakingScore, ScoreSourceType source, Action<AddPvpScoreResponseDto> onSuccess, Action<int> onError)
		{
			var request = CreateDto(new AddPvpScoreRequestDto
			{
				Score = score,
				EventId = GetPvpSeasonTurnId(seasonName, turn),
				MatchMakingScore = matchmakingScore,
				League = league,
				ScoreType = source,
				LuckyCoins = luckyCoins
			});
			StartAsynchPostCall(request, onSuccess, onError);
		}

		public void TrackBossDefeat(string leaderboardId, Action<TrackBossDefeatResponseDto> onSuccess, Action<int> onError)
		{
			var request = CreateDto(new TrackBossDefeatRequestDto
			{
				EventLeaderboardId = leaderboardId
			});
			StartAsynchPostCall(request, onSuccess, onError);
		}

		public string GetPvpSeasonTurnId(string seasonName, int turn)
		{
			return string.Format("{0}_turn_{1}", seasonName, turn.ToString("00"));
		}
	}
}
