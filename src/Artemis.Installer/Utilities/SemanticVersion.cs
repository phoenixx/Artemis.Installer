﻿using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Artemis.Installer.Utilities
{
    /// <summary>
    /// A hybrid implementation of SemVer that supports semantic versioning as described at http://semver.org while not strictly enforcing it to 
    /// allow older 4-digit versioning schemes to continue working.
    /// </summary>
    [Serializable]
    public sealed class SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        private const RegexOptions Flags = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
        private static readonly Regex SemanticVersionRegex = new Regex(@"^(?<Version>\d+(\s*\.\s*\d+){0,3})(?<Release>-[a-z][0-9a-z-.]*)?$", Flags);
        private static readonly Regex StrictSemanticVersionRegex = new Regex(@"^(?<Version>\d+(\.\d+){2})(?<Release>-[a-z][0-9a-z-]*)?$", Flags);
        private readonly string _originalString;
        private string _normalizedVersionString;

        public SemanticVersion(string version)
            : this(Parse(version))
        {
            // The constructor normalizes the version string so that it we do not need to normalize it every time we need to operate on it. 
            // The original string represents the original form in which the version is represented to be used when printing.
            _originalString = version;
        }

        public SemanticVersion(int major, int minor, int build, int revision)
            : this(new Version(major, minor, build, revision))
        {
        }

        public SemanticVersion(int major, int minor, int build, string specialVersion)
            : this(new Version(major, minor, build), specialVersion)
        {
        }

        public SemanticVersion(Version version)
            : this(version, string.Empty)
        {
        }

        public SemanticVersion(Version version, string specialVersion)
            : this(version, specialVersion, null)
        {
        }

        private SemanticVersion(Version version, string specialVersion, string originalString)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }
            Version = NormalizeVersionValue(version);
            SpecialVersion = specialVersion ?? string.Empty;
            _originalString = string.IsNullOrEmpty(originalString) ? version + (!string.IsNullOrEmpty(specialVersion) ? '-' + specialVersion : null) : originalString;
        }

        internal SemanticVersion(SemanticVersion semVer)
        {
            _originalString = semVer.ToString();
            Version = semVer.Version;
            SpecialVersion = semVer.SpecialVersion;
        }

        /// <summary>
        /// Gets the normalized version portion.
        /// </summary>
        public Version Version
        {
            get;
        }

        /// <summary>
        /// Gets the optional special version.
        /// </summary>
        public string SpecialVersion
        {
            get;
        }

        public string[] GetOriginalVersionComponents()
        {
            if (!string.IsNullOrEmpty(_originalString))
            {
                // search the start of the SpecialVersion part, if any
                int dashIndex = _originalString.IndexOf('-');
                string original = dashIndex != -1 ? _originalString.Substring(0, dashIndex) : _originalString;

                return SplitAndPadVersionString(original);
            }

            return SplitAndPadVersionString(Version.ToString());
        }

        private static string[] SplitAndPadVersionString(string version)
        {
            string[] a = version.Split('.');
            if (a.Length == 4)
            {
                return a;
            }

            // if 'a' has less than 4 elements, we pad the '0' at the end 
            // to make it 4.
            string[] b = { "0", "0", "0", "0" };
            Array.Copy(a, 0, b, 0, a.Length);
            return b;
        }

        /// <summary>
        /// Parses a version string using loose semantic versioning rules that allows 2-4 version components followed by an optional special version.
        /// </summary>
        public static SemanticVersion Parse(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentNullException(nameof(version));
            }

            if (!TryParse(version, out SemanticVersion semVer))
            {
                throw new ArgumentException($"{version} is an invalid version number");
            }
            return semVer;
        }

        /// <summary>
        /// Parses a version string using loose semantic versioning rules that allows 2-4 version components followed by an optional special version.
        /// </summary>
        public static bool TryParse(string version, out SemanticVersion value)
        {
            return TryParseInternal(version, SemanticVersionRegex, out value);
        }

        /// <summary>
        /// Parses a version string using strict semantic versioning rules that allows exactly 3 components and an optional special version.
        /// </summary>
        public static bool TryParseStrict(string version, out SemanticVersion value)
        {
            return TryParseInternal(version, StrictSemanticVersionRegex, out value);
        }

        private static bool TryParseInternal(string version, Regex regex, out SemanticVersion semVer)
        {
            semVer = null;
            if (string.IsNullOrEmpty(version))
            {
                return false;
            }

            Match match = regex.Match(version.Trim());
            if (!match.Success || !Version.TryParse(match.Groups["Version"].Value, out Version versionValue))
            {
                return false;
            }

            semVer = new SemanticVersion(NormalizeVersionValue(versionValue), match.Groups["Release"].Value.TrimStart('-'), version.Replace(" ", ""));
            return true;
        }

        /// <summary>
        /// Attempts to parse the version token as a SemanticVersion.
        /// </summary>
        /// <returns>An instance of SemanticVersion if it parses correctly, null otherwise.</returns>
        public static SemanticVersion ParseOptionalVersion(string version)
        {
            TryParse(version, out SemanticVersion semVer);
            return semVer;
        }

        private static Version NormalizeVersionValue(Version version)
        {
            return new Version(version.Major,
                               version.Minor,
                               Math.Max(version.Build, 0),
                               Math.Max(version.Revision, 0));
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 1;
            }
            SemanticVersion other = obj as SemanticVersion;
            if (other == null)
            {
                throw new ArgumentException(nameof(obj));
            }
            return CompareTo(other);
        }

        public int CompareTo(SemanticVersion other)
        {
            if (ReferenceEquals(other, null))
            {
                return 1;
            }

            int result = Version.CompareTo(other.Version);

            if (result != 0)
            {
                return result;
            }

            bool empty = string.IsNullOrEmpty(SpecialVersion);
            bool otherEmpty = string.IsNullOrEmpty(other.SpecialVersion);
            switch (empty)
            {
                case true when otherEmpty:
                    return 0;
                case true:
                    return 1;
                default:
                {
                    if (otherEmpty)
                    {
                        return -1;
                    }

                    break;
                }
            }
            return StringComparer.OrdinalIgnoreCase.Compare(SpecialVersion, other.SpecialVersion);
        }

        public static bool operator ==(SemanticVersion version1, SemanticVersion version2)
        {
            return version1?.Equals(version2) ?? ReferenceEquals(version2, null);
        }

        public static bool operator !=(SemanticVersion version1, SemanticVersion version2)
        {
            return !(version1 == version2);
        }

        public static bool operator <(SemanticVersion version1, SemanticVersion version2)
        {
            if (version1 == null)
            {
                throw new ArgumentNullException(nameof(version1));
            }
            return version1.CompareTo(version2) < 0;
        }

        public static bool operator <=(SemanticVersion version1, SemanticVersion version2)
        {
            return (version1 == version2) || (version1 < version2);
        }

        public static bool operator >(SemanticVersion version1, SemanticVersion version2)
        {
            if (version1 == null)
            {
                throw new ArgumentNullException(nameof(version1));
            }
            return version2 < version1;
        }

        public static bool operator >=(SemanticVersion version1, SemanticVersion version2)
        {
            return (version1 == version2) || (version1 > version2);
        }

        public override string ToString()
        {
            return _originalString;
        }

        /// <summary>
        /// Returns the normalized string representation of this instance of <see cref="SemanticVersion"/>.
        /// If the instance can be strictly parsed as a <see cref="SemanticVersion"/>, the normalized version
        /// string if of the format {major}.{minor}.{build}[-{special-version}]. If the instance has a non-zero
        /// value for revision, the format is {major}.{minor}.{build}.{revision}[-{special-version}].
        /// </summary>
        /// <returns>The normalized string representation.</returns>
        public string ToNormalizedString()
        {
            if (_normalizedVersionString != null) return _normalizedVersionString;
            StringBuilder builder = new StringBuilder();
            builder
                .Append(Version.Major)
                .Append('.')
                .Append(Version.Minor)
                .Append('.')
                .Append(Math.Max(0, Version.Build));

            if (Version.Revision > 0)
            {
                builder.Append('.')
                    .Append(Version.Revision);
            }

            if (!string.IsNullOrEmpty(SpecialVersion))
            {
                builder.Append('-')
                    .Append(SpecialVersion);
            }

            _normalizedVersionString = builder.ToString();

            return _normalizedVersionString;
        }

        public bool Equals(SemanticVersion other)
        {
            return !ReferenceEquals(null, other) &&
                   Version.Equals(other.Version) &&
                   SpecialVersion.Equals(other.SpecialVersion, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is SemanticVersion semVer && Equals(semVer);
        }

        public override int GetHashCode()
        {
            int hashCode = Version.GetHashCode();
            if (SpecialVersion != null)
            {
                hashCode = hashCode * 4567 + SpecialVersion.GetHashCode();
            }

            return hashCode;
        }
    }
}
