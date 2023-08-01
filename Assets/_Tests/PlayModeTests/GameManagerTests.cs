using System.Collections;
using _Project.Code.Scripts;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace _Tests.PlayModeTests
{
    public class GameManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            SceneManager.LoadScene("SampleScene");
        }

        [UnityTest]
        public IEnumerator When_StartNewGame_Expect_LevelIs1_And_ScoreIs0()
        {
            var levelValue = GameObject.Find("Level Value").GetComponent<TextMeshProUGUI>();
            var scoreValue = GameObject.Find("Score Value").GetComponent<TextMeshProUGUI>();

            Assert.AreEqual("1", levelValue.text);
            Assert.AreEqual("0", scoreValue.text);

            yield return null;
        }

        [UnityTest]
        public IEnumerator When_InvokeOnLevelCompleted_LevelIs2()
        {
            var gridSystem = Object.FindObjectOfType<GridSystem>();
            Assert.IsNotNull(gridSystem);

            yield return null;
        }
    }
}
