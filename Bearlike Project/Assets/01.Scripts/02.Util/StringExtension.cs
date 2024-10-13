using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Util
{
    public static class StringExtension
    {
        private static readonly string Pattern = @"\[(.*?)\]";  // 대괄호 안의 문자열을 찾기 위한 정규 표현식
        private static readonly string NumberPattern = @"-?\d+(\.\d+)?";  // 문자열에서 숫자 찾기 위한 정규 표현식
        private static string ComputeAndCalculateNumber(Match m)
        {
            string expression = m.Groups[1].Value;
            try
            {
                var table = new DataTable();
                var result = table.Compute(expression, null);
                
                return result.ToString();  // 계산 결과를 문자열로 변환하여 반환합니다.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating expression '{expression}': {ex.Message}");
                return expression;  // 오류 발생 시 원래의 수식을 반환합니다.
            }
        }
        
        private static string ConvertFloor(Match m)
        {
            string number = m.Value;
            if (number.Contains("."))
            {
                // 부동 소수점 숫자를 정수로 변환
                return ((int)Math.Round(double.Parse(number))).ToString();
            }
            return number;
        }
        
        private static string ConvertNoExistFloor(Match m)
        {
            string number = m.Value;
            if (number.Contains("."))
            {
                // 부동 소수점 숫자를 정수로 변환, 소수 부분이 0일 경우만 변환
                double doubleResult = double.Parse(number);
                if (doubleResult == Math.Floor(doubleResult))
                {
                    return ((int)doubleResult).ToString();
                }
                return number; // 소수 부분이 존재하면 그대로 반환
            }
            return number; // 정
        }

        /// <summary>
        /// 문자열의 부동소수점을 지워준다.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="isExistFloatPoint">부동 수소점이 존재하면 그대로 Int로 바꾸지 않을 것에 대한 여부</param>
        /// <returns></returns>
        public static string Floor(this string result, bool isExistFloatPoint = false)
        {
            return isExistFloatPoint ? Regex.Replace(result, NumberPattern, ConvertNoExistFloor) : Regex.Replace(result, NumberPattern, ConvertFloor);
        }
        
        /// <summary>
        /// 수학적 공식이 담긴 문자열이 있으면 계산해준다.
        /// 단 [] 괄호 안에 담겨 있어야한다.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CalculateNumber(this string value)
        {
            var result = Regex.Replace(value, Pattern, ComputeAndCalculateNumber);
            return result;
        }

        public static string TryReplace(this string value, string replaceTarget, string replaceValue)
        {
            if (value.Contains(replaceTarget))
                return value.Replace(replaceTarget, replaceValue);
            return value;
        }

        /// <summary>
        /// 문자열 길이와 온점(.)을 기준으로 문자열의 1줄 당 길이를 지정
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength">문자열의 1줄의 최대 길이</param>
        /// <returns></returns>
        public static string WrapText(this string value, int maxLength)
        {
            string[] words = value.Split(' ');
            StringBuilder currentLine = new StringBuilder();
            StringBuilder result = new StringBuilder();

            foreach (string word in words)
            {
                if (currentLine.Length + word.Length + 1 > maxLength)
                {
                    result.AppendLine(currentLine.ToString().Trim());
                    currentLine.Clear();
                }

                if (currentLine.Length > 0)
                {
                    currentLine.Append(" ");
                }
                currentLine.Append(word);

                if (word.EndsWith("."))
                {
                    result.AppendLine(currentLine.ToString().Trim());
                    currentLine.Clear();
                }
            }

            if (currentLine.Length > 0)
            {
                result.AppendLine(currentLine.ToString().Trim());
            }

            return result.ToString();
        }
    }
}