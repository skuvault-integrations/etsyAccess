using System.Net;
using System.Net.Http;
using EtsyAccess.Exceptions;
using EtsyAccess.Models.Configuration;
using EtsyAccess.Models.Throttling;
using EtsyAccess.Services;
using EtsyAccess.Shared;
using NUnit.Framework;

namespace EtsyAccessTests
{
	public class BaseServiceTests
	{
		private BaseService _baseService;

		[ SetUp ]
		public void Initialize()
		{
			_baseService = new BaseService( "AA", "BB", new EtsyConfig(), new Throttler( 0, 0 ) );
		}

		[ Test ]
		public void ThrowIfError_WhenStatusCodeOkORCreated_ThenExitsWithoutException( [ Values( HttpStatusCode.OK, HttpStatusCode.Created )] HttpStatusCode statusCode )
		{
			var responseMessage = new HttpResponseMessage { StatusCode = statusCode };

			_baseService.ThrowIfError( responseMessage, "" );
		}

		[ Test ]
		public void ThrowIfError_WhenSignatureInvalid_ThenThrowsEtsyInvalidSignatureException()
		{
			var responseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
			const string message = "signature_invalid";

			Assert.That( () => _baseService.ThrowIfError( responseMessage, message ),
				Throws.TypeOf<EtsyInvalidSignatureException>()
					.With.Message.EqualTo( message ));
		}

		[ Test ]
		public void ThrowIfError_WhenExceededQuota_ThenThrowsEtsyApiLimitsExceededException()
		{
			var responseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
			const string message = "exceeded your quota";

			Assert.That( () => _baseService.ThrowIfError( responseMessage, message ),
				Throws.TypeOf<EtsyApiLimitsExceeded>()
					.With.Message.EqualTo( message ));
		}

		[ Test ]
		public void ThrowIfError_WhenServerException_ThenThrowsEtsyServerException()
		{
			var responseMessage = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };
			const string message = "some random server exception";

			Assert.That( () => _baseService.ThrowIfError( responseMessage, message ),
				Throws.TypeOf<EtsyServerException>()
					.With.Message.EqualTo( message ));
		}

		[ Test ]
		public void CreateMethodCallInfo()
		{
			const string localPath = "/folder";
			const string query = "?param1=a&param2=b";
			var mark = Mark.CreateNew();
			const string errors = "error";
			const string methodResult = "result";
			const string additionalInfo = "addl info";
			const string memberName = "bob";

			var callInfo = _baseService.CreateMethodCallInfo( localPath + query, mark, errors, methodResult, additionalInfo, memberName );

			Assert.AreEqual( string.Format( "{{MethodName: {0}, Mark: '{1}', ServiceEndPoint: '{2}', {3} {4}{5}{6}}}", 
				memberName, mark, localPath, ", RequestParameters: " + query, 
				", Errors:" + errors, ", Result:" + methodResult, ", " + additionalInfo ), callInfo );
		}

		[ Test ]
		public void GetEtsyLimits_WhenRateLimitSupplied_ThenReturnsRateLimit()
		{
			const int rateLimit = 1000;
			const int rateLimitRemaining = 234;
			var response = new HttpResponseMessage();
			response.Headers.Add( "X-RateLimit-Limit", rateLimit.ToString() );
			response.Headers.Add( "X-RateLimit-Remaining", rateLimitRemaining.ToString() );

			var limits = _baseService.GetEtsyLimits( response );

			Assert.AreEqual( rateLimit, limits.TotalAvailableRequests );
			Assert.AreEqual( rateLimitRemaining, limits.CallsRemaining );
		}

		[ Test ]
		public void GetEtsyLimits_WhenRateLimitNotSupplied_ThenReturnsNull()
		{
			var response = new HttpResponseMessage();

			var limits = _baseService.GetEtsyLimits( response );

			Assert.IsNull( limits );
		}
	}
}
