// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Genova.Crawler.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Genova.Crawler.QualityTests;

[TestClass]
public class CodeQuality_Tests : CodeQuality_Base
{
    public CodeQuality_Tests()
        : base(typeof(ResourceDetails).Assembly, "Genova.Crawler")
    {
    }
}
