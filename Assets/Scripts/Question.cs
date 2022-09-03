using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Question : IEquatable<Question>
{
    public string QuestionTS { get; set; }
    public string RightAnswer { get; set; }
    public string[] Answers { get; set; }
    public string Clarification { get; set; }
    public string PathToImage { get; set; }

    public bool Equals(Question other)
    {
        throw new NotImplementedException();
    }
}
