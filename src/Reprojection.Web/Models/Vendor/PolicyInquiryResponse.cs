using System.Text.Json.Serialization;

namespace Reprojection.Web.Models.Vendor;

// ---------------------------------------------------------------------------
// DTOs for the vendor's GET policy response.
//
// These are a STARTING SHAPE for a universal-life style in-force record.
// Replace / adjust them to match the vendor's real JSON. The quickest way:
//   1. Copy a real response to the clipboard.
//   2. In Visual Studio: Edit > Paste Special > Paste JSON As Classes.
//   3. Rename the generated classes and keep the [JsonPropertyName] attributes.
// The "Mapping check" panel on the Policy page will tell you which JSON fields
// are not landing in a model and which model properties the JSON never sent.
//
// Money is decimal (never double). Dates use DateOnly and expect "yyyy-MM-dd";
// if the vendor sends another format, add a converter in VendorJson.
// ---------------------------------------------------------------------------

public sealed class PolicyInquiryResponse
{
    [JsonPropertyName("requestId")]          public string? RequestId { get; set; }
    [JsonPropertyName("responseTimestamp")]  public DateTimeOffset? ResponseTimestamp { get; set; }
    [JsonPropertyName("policy")]             public PolicyInfo? Policy { get; set; }
}

public sealed class PolicyInfo
{
    [JsonPropertyName("policyNumber")]       public string? PolicyNumber { get; set; }
    [JsonPropertyName("productCode")]        public string? ProductCode { get; set; }
    [JsonPropertyName("productName")]        public string? ProductName { get; set; }
    [JsonPropertyName("planType")]           public string? PlanType { get; set; }          // UL, IUL, VUL, WL, TERM
    [JsonPropertyName("status")]             public string? Status { get; set; }            // Active, Lapsed, Paid-Up ...
    [JsonPropertyName("issueDate")]          public DateOnly? IssueDate { get; set; }
    [JsonPropertyName("issueState")]         public string? IssueState { get; set; }
    [JsonPropertyName("maturityDate")]       public DateOnly? MaturityDate { get; set; }
    [JsonPropertyName("valuationDate")]      public DateOnly? ValuationDate { get; set; }
    [JsonPropertyName("paidToDate")]         public DateOnly? PaidToDate { get; set; }
    [JsonPropertyName("billingMode")]        public string? BillingMode { get; set; }       // Annual, Semi-Annual, Quarterly, Monthly
    [JsonPropertyName("modalPremium")]       public decimal? ModalPremium { get; set; }
    [JsonPropertyName("plannedPremium")]     public decimal? PlannedPremium { get; set; }
    [JsonPropertyName("deathBenefitOption")] public string? DeathBenefitOption { get; set; } // A (level), B (increasing), C
    [JsonPropertyName("taxComplianceTest")]  public string? TaxComplianceTest { get; set; }  // GPT / CVAT

    [JsonPropertyName("owner")]              public PolicyOwner? Owner { get; set; }
    [JsonPropertyName("insureds")]           public List<Insured> Insureds { get; set; } = [];
    [JsonPropertyName("coverages")]          public List<Coverage> Coverages { get; set; } = [];
    [JsonPropertyName("riders")]             public List<Rider> Riders { get; set; } = [];
    [JsonPropertyName("values")]             public PolicyValues? Values { get; set; }
    [JsonPropertyName("loans")]              public List<PolicyLoan> Loans { get; set; } = [];
    [JsonPropertyName("funds")]              public List<FundAllocation> Funds { get; set; } = [];
}

public sealed class PolicyOwner
{
    [JsonPropertyName("partyId")]   public string? PartyId { get; set; }
    [JsonPropertyName("name")]      public string? Name { get; set; }
    [JsonPropertyName("ownerType")] public string? OwnerType { get; set; }   // Individual, Trust, Corporation
    [JsonPropertyName("state")]     public string? State { get; set; }
}

public sealed class Insured
{
    [JsonPropertyName("partyId")]     public string? PartyId { get; set; }
    [JsonPropertyName("role")]        public string? Role { get; set; }        // Primary, Joint, Other
    [JsonPropertyName("firstName")]   public string? FirstName { get; set; }
    [JsonPropertyName("lastName")]    public string? LastName { get; set; }
    [JsonPropertyName("gender")]      public string? Gender { get; set; }
    [JsonPropertyName("dateOfBirth")] public DateOnly? DateOfBirth { get; set; }
    [JsonPropertyName("issueAge")]    public int? IssueAge { get; set; }
    [JsonPropertyName("attainedAge")] public int? AttainedAge { get; set; }
    [JsonPropertyName("riskClass")]   public string? RiskClass { get; set; }   // Preferred Plus, Standard ...
    [JsonPropertyName("tobaccoUse")]  public bool? TobaccoUse { get; set; }
    [JsonPropertyName("tableRating")] public string? TableRating { get; set; }
    [JsonPropertyName("flatExtraPerThousand")] public decimal? FlatExtraPerThousand { get; set; }

    [JsonIgnore] public string FullName => $"{FirstName} {LastName}".Trim();
}

public sealed class Coverage
{
    [JsonPropertyName("coverageId")]     public string? CoverageId { get; set; }
    [JsonPropertyName("phase")]          public int? Phase { get; set; }
    [JsonPropertyName("planCode")]       public string? PlanCode { get; set; }
    [JsonPropertyName("description")]    public string? Description { get; set; }
    [JsonPropertyName("insuredPartyId")] public string? InsuredPartyId { get; set; }
    [JsonPropertyName("faceAmount")]     public decimal? FaceAmount { get; set; }
    [JsonPropertyName("issueDate")]      public DateOnly? IssueDate { get; set; }
    [JsonPropertyName("status")]         public string? Status { get; set; }
    [JsonPropertyName("riskClass")]      public string? RiskClass { get; set; }
}

public sealed class Rider
{
    [JsonPropertyName("riderCode")]   public string? RiderCode { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("amount")]      public decimal? Amount { get; set; }
    [JsonPropertyName("issueDate")]   public DateOnly? IssueDate { get; set; }
    [JsonPropertyName("expiryDate")]  public DateOnly? ExpiryDate { get; set; }
    [JsonPropertyName("status")]      public string? Status { get; set; }
    [JsonPropertyName("monthlyCharge")] public decimal? MonthlyCharge { get; set; }
}

public sealed class PolicyValues
{
    [JsonPropertyName("accountValue")]           public decimal? AccountValue { get; set; }
    [JsonPropertyName("cashSurrenderValue")]     public decimal? CashSurrenderValue { get; set; }
    [JsonPropertyName("surrenderCharge")]        public decimal? SurrenderCharge { get; set; }
    [JsonPropertyName("netDeathBenefit")]        public decimal? NetDeathBenefit { get; set; }
    [JsonPropertyName("loanBalance")]            public decimal? LoanBalance { get; set; }
    [JsonPropertyName("accruedLoanInterest")]    public decimal? AccruedLoanInterest { get; set; }
    [JsonPropertyName("currentInterestRate")]    public decimal? CurrentInterestRate { get; set; }    // percent, e.g. 4.25
    [JsonPropertyName("guaranteedInterestRate")] public decimal? GuaranteedInterestRate { get; set; }
    [JsonPropertyName("cumulativePremiumsPaid")] public decimal? CumulativePremiumsPaid { get; set; }
    [JsonPropertyName("costBasis")]              public decimal? CostBasis { get; set; }
    [JsonPropertyName("targetPremium")]          public decimal? TargetPremium { get; set; }
    [JsonPropertyName("minimumPremium")]         public decimal? MinimumPremium { get; set; }
    [JsonPropertyName("guidelineSinglePremium")] public decimal? GuidelineSinglePremium { get; set; }
    [JsonPropertyName("guidelineLevelPremium")]  public decimal? GuidelineLevelPremium { get; set; }
    [JsonPropertyName("sevenPayPremium")]        public decimal? SevenPayPremium { get; set; }
    [JsonPropertyName("isMec")]                  public bool? IsMec { get; set; }
}

public sealed class PolicyLoan
{
    [JsonPropertyName("loanType")]        public string? LoanType { get; set; }   // Standard, Preferred, Variable
    [JsonPropertyName("principal")]       public decimal? Principal { get; set; }
    [JsonPropertyName("interestRate")]    public decimal? InterestRate { get; set; }
    [JsonPropertyName("accruedInterest")] public decimal? AccruedInterest { get; set; }
    [JsonPropertyName("loanDate")]        public DateOnly? LoanDate { get; set; }
}

public sealed class FundAllocation
{
    [JsonPropertyName("fundCode")]          public string? FundCode { get; set; }
    [JsonPropertyName("fundName")]          public string? FundName { get; set; }
    [JsonPropertyName("allocationPercent")] public decimal? AllocationPercent { get; set; }
    [JsonPropertyName("balance")]           public decimal? Balance { get; set; }
    [JsonPropertyName("creditedRate")]      public decimal? CreditedRate { get; set; }
}
