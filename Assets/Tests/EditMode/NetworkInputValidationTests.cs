using NUnit.Framework;

namespace UnityMultiV2.Tests
{
    public class NetworkInputValidationTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void TryNormalizeChatMessage_RejectsEmptyInput(string message)
        {
            bool accepted = NetworkInputValidation.TryNormalizeChatMessage(
                message,
                out string normalizedMessage
            );

            Assert.That(accepted, Is.False);
            Assert.That(normalizedMessage, Is.Empty);
        }

        [Test]
        public void TryNormalizeChatMessage_TrimsAndLimitsLength()
        {
            string message = "  " + new string('a', 300) + "  ";

            bool accepted = NetworkInputValidation.TryNormalizeChatMessage(
                message,
                out string normalizedMessage
            );

            Assert.That(accepted, Is.True);
            Assert.That(
                normalizedMessage.Length,
                Is.EqualTo(NetworkInputValidation.MaximumChatMessageLength)
            );
        }

        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(50, true)]
        [TestCase(51, false)]
        public void IsValidDamage_EnforcesAllowedRange(
            int damage,
            bool expected)
        {
            Assert.That(
                NetworkInputValidation.IsValidDamage(damage),
                Is.EqualTo(expected)
            );
        }
    }
}
