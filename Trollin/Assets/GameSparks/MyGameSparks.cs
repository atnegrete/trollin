#pragma warning disable 612,618
#pragma warning disable 0114
#pragma warning disable 0108

using System;
using System.Collections.Generic;
using GameSparks.Core;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;

//THIS FILE IS AUTO GENERATED, DO NOT MODIFY!!
//THIS FILE IS AUTO GENERATED, DO NOT MODIFY!!
//THIS FILE IS AUTO GENERATED, DO NOT MODIFY!!

namespace GameSparks.Api.Requests{
		public class LogEventRequest_GET_Match_Player_Details_By_Id : GSTypedRequest<LogEventRequest_GET_Match_Player_Details_By_Id, LogEventResponse>
	{
	
		protected override GSTypedResponse BuildResponse (GSObject response){
			return new LogEventResponse (response);
		}
		
		public LogEventRequest_GET_Match_Player_Details_By_Id() : base("LogEventRequest"){
			request.AddString("eventKey", "GET_Match_Player_Details_By_Id");
		}
		
		public LogEventRequest_GET_Match_Player_Details_By_Id Set_MATCHID( string value )
		{
			request.AddString("MATCHID", value);
			return this;
		}
	}
	
	public class LogChallengeEventRequest_GET_Match_Player_Details_By_Id : GSTypedRequest<LogChallengeEventRequest_GET_Match_Player_Details_By_Id, LogChallengeEventResponse>
	{
		public LogChallengeEventRequest_GET_Match_Player_Details_By_Id() : base("LogChallengeEventRequest"){
			request.AddString("eventKey", "GET_Match_Player_Details_By_Id");
		}
		
		protected override GSTypedResponse BuildResponse (GSObject response){
			return new LogChallengeEventResponse (response);
		}
		
		/// <summary>
		/// The challenge ID instance to target
		/// </summary>
		public LogChallengeEventRequest_GET_Match_Player_Details_By_Id SetChallengeInstanceId( String challengeInstanceId )
		{
			request.AddString("challengeInstanceId", challengeInstanceId);
			return this;
		}
		public LogChallengeEventRequest_GET_Match_Player_Details_By_Id Set_MATCHID( string value )
		{
			request.AddString("MATCHID", value);
			return this;
		}
	}
	
	public class LogEventRequest_GET_Player_Details : GSTypedRequest<LogEventRequest_GET_Player_Details, LogEventResponse>
	{
	
		protected override GSTypedResponse BuildResponse (GSObject response){
			return new LogEventResponse (response);
		}
		
		public LogEventRequest_GET_Player_Details() : base("LogEventRequest"){
			request.AddString("eventKey", "GET_Player_Details");
		}
	}
	
	public class LogChallengeEventRequest_GET_Player_Details : GSTypedRequest<LogChallengeEventRequest_GET_Player_Details, LogChallengeEventResponse>
	{
		public LogChallengeEventRequest_GET_Player_Details() : base("LogChallengeEventRequest"){
			request.AddString("eventKey", "GET_Player_Details");
		}
		
		protected override GSTypedResponse BuildResponse (GSObject response){
			return new LogChallengeEventResponse (response);
		}
		
		/// <summary>
		/// The challenge ID instance to target
		/// </summary>
		public LogChallengeEventRequest_GET_Player_Details SetChallengeInstanceId( String challengeInstanceId )
		{
			request.AddString("challengeInstanceId", challengeInstanceId);
			return this;
		}
	}
	
	public class LogEventRequest_UP_Player_Details : GSTypedRequest<LogEventRequest_UP_Player_Details, LogEventResponse>
	{
	
		protected override GSTypedResponse BuildResponse (GSObject response){
			return new LogEventResponse (response);
		}
		
		public LogEventRequest_UP_Player_Details() : base("LogEventRequest"){
			request.AddString("eventKey", "UP_Player_Details");
		}
		public LogEventRequest_UP_Player_Details Set_RED( long value )
		{
			request.AddNumber("RED", value);
			return this;
		}			
		public LogEventRequest_UP_Player_Details Set_GREEN( long value )
		{
			request.AddNumber("GREEN", value);
			return this;
		}			
		public LogEventRequest_UP_Player_Details Set_BLUE( long value )
		{
			request.AddNumber("BLUE", value);
			return this;
		}			
	}
	
	public class LogChallengeEventRequest_UP_Player_Details : GSTypedRequest<LogChallengeEventRequest_UP_Player_Details, LogChallengeEventResponse>
	{
		public LogChallengeEventRequest_UP_Player_Details() : base("LogChallengeEventRequest"){
			request.AddString("eventKey", "UP_Player_Details");
		}
		
		protected override GSTypedResponse BuildResponse (GSObject response){
			return new LogChallengeEventResponse (response);
		}
		
		/// <summary>
		/// The challenge ID instance to target
		/// </summary>
		public LogChallengeEventRequest_UP_Player_Details SetChallengeInstanceId( String challengeInstanceId )
		{
			request.AddString("challengeInstanceId", challengeInstanceId);
			return this;
		}
		public LogChallengeEventRequest_UP_Player_Details Set_RED( long value )
		{
			request.AddNumber("RED", value);
			return this;
		}			
		public LogChallengeEventRequest_UP_Player_Details Set_GREEN( long value )
		{
			request.AddNumber("GREEN", value);
			return this;
		}			
		public LogChallengeEventRequest_UP_Player_Details Set_BLUE( long value )
		{
			request.AddNumber("BLUE", value);
			return this;
		}			
	}
	
}
	

namespace GameSparks.Api.Messages {


}
