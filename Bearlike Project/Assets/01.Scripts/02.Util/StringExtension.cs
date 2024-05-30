using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Util
{
    public static class StringExtension
    {
        public static readonly string Pattern = @"\[(.*?)\]";  // 대괄호 안의 문자열을 찾기 위한 정규 표현식
        
        public static string ComputeAndReplace(Match m)
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

        public static string Replace(string value)
        {
            return Regex.Replace(value, Pattern, ComputeAndReplace);
        }
    }
}