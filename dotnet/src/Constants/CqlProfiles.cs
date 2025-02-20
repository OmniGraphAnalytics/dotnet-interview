using Cassandra;

namespace OmniGraphInterview.Constants;

/// <summary>
/// CQL Execution Profiles.
/// </summary>
public static class CqlProfiles
{
    /// <summary>
    /// The default profile. This is used when no profile is specified.
    /// Uses ConsistencyLevel.LocalOne.
    /// </summary>
    public static readonly Profile Default = new()
    {
        Name = "default",
        Build = profile => profile.WithConsistencyLevel(ConsistencyLevel.LocalQuorum),
    };

    /// <summary>
    /// A fast read profile. Uses ConsistencyLevel.One.
    /// </summary>
    public static readonly Profile ReadFast = new()
    {
        Name = "read_fast",
        Build = profile => profile.WithConsistencyLevel(ConsistencyLevel.One),
    };

    /// <summary>
    /// A safe read profile. Uses ConsistencyLevel.LocalQuorum.
    /// </summary>
    public static readonly Profile ReadSafe = new()
    {
        Name = "read_safe",
        Build = profile => profile.WithConsistencyLevel(ConsistencyLevel.LocalQuorum),
    };

    /// <summary>
    /// A safe write profile. Uses ConsistencyLevel.Quorum.
    /// </summary>
    public static readonly Profile WriteSafe = new()
    {
        Name = "write_safe",
        Build = profile => profile.WithConsistencyLevel(ConsistencyLevel.Quorum),
    };

    /// <summary>
    /// Helper method to build and implement the profiles.
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// The name of the profile.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The action to build the profile.
        /// </summary>
        public required Action<IExecutionProfileBuilder> Build { get; set; }

        /// <summary>
        /// Builds the profile and attaches it to the options.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IExecutionProfileOptions AttachTo(IExecutionProfileOptions options)
        {
            return options.WithProfile(name: Name, profileBuildAction: Build);
        }
    }

}
