using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrueVote.Api.Helpers;

namespace TrueVote.Api.Models
{
    public class ElectionModelList
    {
        [Required]
        [MaxLength(2048)]
        [Description("List of Elections")]
        [JsonPropertyName("Elections")]
        [JsonProperty(nameof(Elections), Required = Required.Always)]
        public required List<ElectionModel> Elections { get; set; }
    }

    public class AddRacesModel
    {
        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ElectionId")]
        [JsonProperty(nameof(ElectionId), Required = Required.Always)]
        [Key]
        public required string ElectionId { get; set; }

        [Required]
        [Description("Race Ids")]
        [JsonPropertyName("RaceIds")]
        [JsonProperty(nameof(RaceIds), Required = Required.Always)]
        public required List<string> RaceIds { get; set; } = new List<string>();
    }

    public class FindElectionModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public required string Name { get; set; } = string.Empty;
    }

    public abstract class RootElectionBaseModel
    {
        [Required]
        [Description("Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Name")]
        [JsonProperty(nameof(Name), Required = Required.Always)]
        public required string Name { get; set; } = string.Empty;

        [Required]
        [Description("Description")]
        [MaxLength(32768)]
        [DataType(DataType.Text)]
        [JsonPropertyName("Description")]
        [JsonProperty(nameof(Description), Required = Required.Always)]
        public required string Description { get; set; } = string.Empty;

        [Required]
        [Description("HeaderImageUrl")]
        [MaxLength(1024)]
        [DataType(DataType.Text)]
        [JsonPropertyName("HeaderImageUrl")]
        [JsonProperty(nameof(HeaderImageUrl), Required = Required.Always)]
        public required string HeaderImageUrl { get; set; } = string.Empty;

        [Required]
        [Description("StartDate")]
        [DataType(DataType.Date)]
        [JsonPropertyName("StartDate")]
        [JsonProperty(nameof(StartDate), Required = Required.Always)]
        public required DateTime StartDate { get; set; }

        [Required]
        [Description("EndDate")]
        [DataType(DataType.Date)]
        [JsonPropertyName("EndDate")]
        [JsonProperty(nameof(EndDate), Required = Required.Always)]
        public required DateTime EndDate { get; set; }
    }

    public class BaseElectionModel : RootElectionBaseModel
    {
        [Required]
        [Description("List of BaseRaces")]
        [DataType("List<BaseRaceModel>")]
        [JsonPropertyName("Races")]
        [JsonProperty(nameof(BaseRaces), Required = Required.Always)]
        public required List<BaseRaceModel> BaseRaces { get; set; } = new List<BaseRaceModel>();
    }

    public class ElectionModel : RootElectionBaseModel
    {
        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ElectionId")]
        [JsonProperty(nameof(ElectionId), Required = Required.Always)]
        [Key]
        public required string ElectionId { get; set; }

        [Description("Parent Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ParentElectionId")]
        [JsonProperty(nameof(ParentElectionId), Required = Required.Default)]
        public string? ParentElectionId { get; set; }

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        public required DateTime DateCreated { get; set; }

        [Required]
        [Description("List of Races")]
        [DataType("List<RaceModel>")]
        [JsonPropertyName("Races")]
        [JsonProperty(nameof(Races), Required = Required.Always)]
        public required List<RaceModel> Races { get; set; } = new List<RaceModel>();
    }

    public static class ElectionModelExtensions
    {
        public static ElectionModel DTOToElection(this BaseElectionModel baseElection, List<RaceModel> races)
        {
            var election = new ElectionModel
            {
                ElectionId = Guid.NewGuid().ToString(),
                Name = baseElection.Name,
                Description = baseElection.Description,
                HeaderImageUrl = baseElection.HeaderImageUrl,
                StartDate = baseElection.StartDate,
                EndDate = baseElection.EndDate,
                Races = races,
                DateCreated = UtcNowProviderFactory.GetProvider().UtcNow
            };

            return election;
        }
    }

    [PrimaryKey(nameof(RequestId), [nameof(ElectionId), nameof(AccessCode)])]
    public class AccessCodeModel
    {
        [Required]
        [Description("Request Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("RequestId")]
        [JsonProperty(nameof(RequestId), Required = Required.Always)]
        [Key]
        public required string RequestId { get; set; }

        [Required]
        [Description("Request Description")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("RequestDescription")]
        [JsonProperty(nameof(RequestDescription), Required = Required.Always)]
        public required string RequestDescription { get; set; }

        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ElectionId")]
        [JsonProperty(nameof(ElectionId), Required = Required.Always)]
        [Key]
        public required string ElectionId { get; set; }

        [Required]
        [Description("Requested By User Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("RequestedByUserId")]
        [JsonProperty(nameof(RequestedByUserId), Required = Required.Always)]
        public required string RequestedByUserId { get; set; }

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        public required DateTime DateCreated { get; set; }

        [Required]
        [Description("Access Code")]
        [MaxLength(16)]
        [DataType(DataType.Text)]
        [JsonPropertyName("AccessCode")]
        [JsonProperty(nameof(AccessCode), Required = Required.Always)]
        [Key]
        public required string AccessCode { get; set; }
    }

    // For DateCreated, only going to store YYYYMMDD, not the time. Because if we stored a very precise date,
    // it would be possible to bind to a BallotId which could reveal the User by using heuristics.
    public class UsedAccessCodeModel
    {
        [Required]
        [Description("Access Code")]
        [MaxLength(16)]
        [DataType(DataType.Text)]
        [JsonPropertyName("AccessCode")]
        [JsonProperty(nameof(AccessCode), Required = Required.Always)]
        [Key]
        public required string AccessCode { get; set; }

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        public required DateTime DateCreated { get; set; }
    }

    // For DateCreated, only going to store YYYYMMDD, not the time. Because if we stored a very precise date,
    // it would be possible to bind to a BallotId which could reveal the User by using heuristics.
    [PrimaryKey(nameof(ElectionId), [nameof(UserId)])]
    public class ElectionUserBindingModel
    {
        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ElectionId")]
        [JsonProperty(nameof(ElectionId), Required = Required.Always)]
        [Key]
        public required string ElectionId { get; set; }

        [Required]
        [Description("User Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("UserId")]
        [JsonProperty(nameof(UserId), Required = Required.Always)]
        [Key]
        public required string UserId { get; set; }

        [Required]
        [Description("DateCreated")]
        [DataType(DataType.Date)]
        [JsonPropertyName("DateCreated")]
        [JsonProperty(nameof(DateCreated), Required = Required.Default)]
        public required DateTime DateCreated { get; set; }
    }

    public class AccessCodesResponse
    {
        [Required]
        [Description("Request Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("RequestId")]
        [JsonProperty(nameof(RequestId), Required = Required.Always)]
        [Key]
        public required string RequestId { get; set; }

        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ElectionId")]
        [JsonProperty(nameof(ElectionId), Required = Required.Always)]
        public required string ElectionId { get; set; }

        [Required]
        [Description("List of Access Codes")]
        [DataType("List<AccessCodeModel>")]
        [JsonPropertyName("AccessCodes")]
        [JsonProperty(nameof(AccessCodes), Required = Required.Always)]
        public required List<AccessCodeModel> AccessCodes { get; set; } = [];
    }

    public class AccessCodesRequest
    {
        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ElectionId")]
        [JsonProperty(nameof(ElectionId), Required = Required.Always)]
        [Key]
        public required string ElectionId { get; set; }

        [Required]
        [Description("User Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("UserId")]
        [JsonProperty(nameof(UserId), Required = Required.Always)]
        public required string UserId { get; set; }

        [Required]
        [Description("Request Description")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("RequestDescription")]
        [JsonProperty(nameof(RequestDescription), Required = Required.Always)]
        public required string RequestDescription { get; set; }

        [Required]
        [Description("Number of Access Codes")]
        [Range(0, int.MaxValue)]
        [JsonPropertyName("NumberOfAccessCodes")]
        [JsonProperty(nameof(NumberOfAccessCodes), Required = Required.Always)]
        public required int? NumberOfAccessCodes { get; set; }
    }

    public class CheckCodeRequest
    {
        [Required]
        [Description("User Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("UserId")]
        [JsonProperty(nameof(UserId), Required = Required.Always)]
        public required string UserId { get; set; }

        [Required]
        [Description("AccessCode")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("AccessCode")]
        [JsonProperty(nameof(AccessCode), Required = Required.Always)]
        public required string AccessCode { get; set; }
    }

    [SwaggerSchema]
    public class ElectionResults
    {
        [Required]
        [Description("Election Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("ElectionId")]
        [JsonProperty(nameof(ElectionId), Required = Required.Always)]
        public required string ElectionId { get; set; }

        [Required]
        [Description("Total number of ballots cast")]
        [Range(0, int.MaxValue)]
        [JsonPropertyName("TotalBallots")]
        [JsonProperty(nameof(TotalBallots), Required = Required.Always)]
        public required int TotalBallots { get; set; }

        [Required]
        [Description("List of Race Results")]
        [DataType("List<RaceResult>")]
        [JsonPropertyName("Races")]
        [JsonProperty(nameof(Races), Required = Required.Always)]
        public required List<RaceResult> Races { get; set; } = new List<RaceResult>();
    }

    [SwaggerSchema]
    public class RaceResult
    {
        [Required]
        [Description("Race Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("RaceId")]
        [JsonProperty(nameof(RaceId), Required = Required.Always)]
        public required string RaceId { get; set; }

        [Required]
        [Description("Race Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("RaceName")]
        [JsonProperty(nameof(RaceName), Required = Required.Always)]
        public required string RaceName { get; set; }

        [Required]
        [Description("List of Candidate Results")]
        [DataType("List<CandidateResult>")]
        [JsonPropertyName("CandidateResults")]
        [JsonProperty(nameof(CandidateResults), Required = Required.Always)]
        public required List<CandidateResult> CandidateResults { get; set; } = new List<CandidateResult>();
    }

    [SwaggerSchema]
    public class CandidateResult
    {
        [Required]
        [Description("Candidate Id")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("CandidateId")]
        [JsonProperty(nameof(CandidateId), Required = Required.Always)]
        public required string CandidateId { get; set; }

        [Required]
        [Description("Candidate Name")]
        [MaxLength(2048)]
        [DataType(DataType.Text)]
        [JsonPropertyName("CandidateName")]
        [JsonProperty(nameof(CandidateName), Required = Required.Always)]
        public required string CandidateName { get; set; }

        [Required]
        [Description("Total votes received by the candidate")]
        [Range(0, int.MaxValue)]
        [JsonPropertyName("TotalVotes")]
        [JsonProperty(nameof(TotalVotes), Required = Required.Always)]
        public required int TotalVotes { get; set; }
    }
}
