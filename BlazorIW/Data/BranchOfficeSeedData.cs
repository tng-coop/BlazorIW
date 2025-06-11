namespace BlazorIW.Data;

public record BranchOfficeSeed(string PostalCode, int Count);

public static class BranchOfficeSeedData
{
    public static readonly BranchOfficeSeed[] Data = new[]
    {
        new BranchOfficeSeed("210-0814", 1),
        new BranchOfficeSeed("210-0833", 1),
        new BranchOfficeSeed("211-0051", 1),
        new BranchOfficeSeed("213-0032", 1),
        new BranchOfficeSeed("226-0016", 1),
        new BranchOfficeSeed("226-0029", 1),
        new BranchOfficeSeed("227-0036", 1),
        new BranchOfficeSeed("230-0001", 1),
        new BranchOfficeSeed("231-0026", 1),
        new BranchOfficeSeed("238-0224", 1),
        new BranchOfficeSeed("240-0026", 1),
        new BranchOfficeSeed("240-0067", 1),
        new BranchOfficeSeed("243-0402", 1),
        new BranchOfficeSeed("244-0003", 3),
        new BranchOfficeSeed("245-0014", 1),
        new BranchOfficeSeed("245-0061", 1),
        new BranchOfficeSeed("245-0062", 1),
        new BranchOfficeSeed("251-0035", 1),
        new BranchOfficeSeed("251-0043", 1),
        new BranchOfficeSeed("251-0047", 1),
        new BranchOfficeSeed("251-0875", 1),
        new BranchOfficeSeed("252-0025", 1),
        new BranchOfficeSeed("252-0303", 1),
        new BranchOfficeSeed("252-0321", 1),
        new BranchOfficeSeed("252-0802", 4),
        new BranchOfficeSeed("252-0813", 3),
        new BranchOfficeSeed("254-0014", 1),
        new BranchOfficeSeed("254-0018", 1),
        new BranchOfficeSeed("254-0061", 1),
        new BranchOfficeSeed("254-0084", 1),
        new BranchOfficeSeed("254-0813", 1),
        new BranchOfficeSeed("254-0906", 2),
        new BranchOfficeSeed("259-1205", 1),
        new BranchOfficeSeed("258-0017", 1)
    };
}
