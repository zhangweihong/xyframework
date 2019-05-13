using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 时间工具函数
/// 为了方便各种格式的时间
/// </summary>
public class TimeUtil
{
    public static string GetTimeToString(TimeSpan leftTime)
    {
        return StringUtil.AppendFormat("{0}:{1}:{2}", leftTime.Hours.ToString("00"), leftTime.Minutes.ToString("00"), leftTime.Seconds.ToString("00"));
    }
    public static string GetTimeToString(DateTime leftTime)
    {
        return StringUtil.AppendFormat("{0}:{1}:{2}", leftTime.Hour.ToString("00"), leftTime.Minute.ToString("00"), leftTime.Second.ToString("00"));
    }

    public static string GetHourMinute2(uint s)
    {
        int ss = 1;//秒
        int mi = ss * 60;//1分60秒
        int hh = mi * 60;//1小时3600秒
        int dd = hh * 24;//1天24*3600秒

        long hour = s / hh;
        long minute = (s - hour * hh) / mi;
        long second = (s - hour * hh - minute * mi);

        if (minute > 0)
        {
            return StringUtil.AppendFormat("{0}分钟", minute);
        }
        else
        {
            return StringUtil.AppendFormat("{0}秒", second);
        }
    }

    /// <summary>
    /// 小于24小时显示XX小时 大于24小时，则显示“1天前离线”省略小
    /// </summary>
    /// <returns></returns>
    public static string GetLeaveTime(uint s)
    {
        uint daySecndTime = 3600 * 24;//一天
        if (s < daySecndTime)
        {
            return GetHourMinute(s);
        }
        else
        {
            uint day = s / daySecndTime;
            return StringUtil.AppendFormat("{0}天", day); //返回xx天
        }
    }

    //xx时xx分
    public static string GetHourMinute(uint s)
    {
        int ss = 1;//秒
        int mi = ss * 60;//1分60秒
        int hh = mi * 60;//1小时3600秒
        int dd = hh * 24;//1天24*3600秒

        long hour = s / hh;
        long minute = (s - hour * hh) / mi;
        return StringUtil.AppendFormat("{0}时{1}分", hour, minute);
    }

    //将秒数换算成 x天x时x分x秒
    public static String getDay(uint s)
    {
        int ss = 1;//秒
        int mi = ss * 60;//1分60秒
        int hh = mi * 60;//1小时3600秒
        int dd = hh * 24;//1天24*3600秒

        long day = s / dd;
        long hour = (s - day * dd) / hh;
        long minute = (s - day * dd - hour * hh) / mi;
        long second = (s - day * dd - hour * hh - minute * mi) / ss;
        //long milliSecond = s - day * dd - hour * hh - minute * mi - second * ss;

        String strDay = day < 10 ? "0" + day : day.ToString();
        String strHour = hour < 10 ? "0" + hour : hour.ToString();
        String strMinute = minute < 10 ? "0" + minute : minute.ToString();
        String strSecond = second < 10 ? "0" + second : second.ToString();

        return StringUtil.AppendFormat("{0}天{1}时{2}分{3}秒", strDay, strHour, strMinute, strSecond);
    }
    /// <summary>
    /// 转换成中文日期
    /// </summary>
    /// <param name="time"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static String ToChineseString(TimeSpan time, int format)
    {
        if (format > 4) format = 4;
        int[] temp = new int[4];
        temp[0] = time.Days;
        temp[1] = time.Hours;
        temp[2] = time.Minutes;
        temp[3] = time.Seconds;
        string[] sts = new string[4];
        sts[0] = "天";
        sts[1] = "时";
        sts[2] = "分";
        sts[3] = "秒";

        int start = 0;
        string resault = "";
        for (int i = 0; i < 4; i++)
        {
            if (temp[i] > 0)
            {
                start = i;
                break;
            }
        }
        if (start > 4 - format) start = 4 - format;
        for (int i = start; i < format + start; i++)
        {
            resault = StringUtil.AppendFormat("{0}{1}", temp[i].ToString("00"), sts[i]);
        }
        return resault;
    }

    //参数 秒  返回 00:00:00   
    static public string getDate(uint s_)
    {
        uint h = s_ / 3600;
        uint m = (s_ % 3600) / 60;
        uint s = s_ % 60;
        return string.Format("{0}:{1}:{2}", (h > 9 ? h.ToString() : string.Format("0{0}", h)), (m > 9 ? m.ToString() : string.Format("0{0}", m)), (s > 9 ? s.ToString() : string.Format("0{0}", s)));
    }
    //参数 秒  返回 00:00   
    static public string getTime(uint s_)
    {
        uint m = (s_ % 3600) / 60;
        uint s = s_ % 60;
        return string.Format("{0}:{1}", (m > 9 ? m.ToString() : string.Format("0{0}", m)), (s > 9 ? s.ToString() : string.Format("0{0}", s)));
    }

    //获取1970/1/1到指定时间的毫秒数如果参数为空用系统当前时间
    static public double getTimeMilliseconds(DateTime time)
    {
        if (time == null) time = DateTime.Now;
        return time.Subtract(DateTime.Parse("1970-1-1")).TotalMilliseconds;
    }

    //获取该时间到当前时间所过的毫秒数
    static public double getOldMillSecounds(DateTime old)
    {
        return getTimeMilliseconds(DateTime.Now) - getTimeMilliseconds(old);
    }

    //秒 转 DateTime
    static public DateTime GetDateTimeBySeconds(double seconds)
    {
        return new DateTime(1970, 1, 1, 8, 0, 0, 0).AddSeconds(seconds);
    }
    /// <summary>
    /// 毫秒转DateTime
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    static public DateTime GetDateTimeByMilliSeconds(ulong seconds)
    {
        return new DateTime(1970, 1, 1, 8, 0, 0, 0).AddMilliseconds(seconds);
    }

    /// <summary>
    /// 获取当前本地时间戳
    /// </summary>
    /// <returns></returns>      
    public static long GetCurrentTimeUnix()
    {
        TimeSpan cha = (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
        long t = (long)cha.TotalSeconds;
        return t;
    }
    /// <summary>
    /// 时间戳转换为本地时间对象
    /// </summary>
    /// <returns></returns>      
    public static DateTime GetUnixDateTime(long unix)
    {
        //long unix = 1500863191;
        DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        DateTime newTime = dtStart.AddSeconds(unix);
        return newTime;
    }

    /// <summary>
    /// 判断对比某个时间节点是否过了一天
    /// </summary>
    /// <param name="remindDate"></param>
    /// <returns></returns>
    public static bool JudgeIfPassOneDay(DateTime remindDate)
    {
        if (remindDate != null)
        {
            int remindDay = remindDate.Day;
            int remindMonth = remindDate.Month;
            int remindYear = remindDate.Year;

            if (DateTime.Now.Year > remindYear) //判断是否过了一天了
            {
                return true;
            }
            else
            {
                if (DateTime.Now.Year < remindYear)
                {
                    Debug.Log("Year >>> 提醒时间或者当前系统时间有问题");
                }
                else
                {
                    if (DateTime.Now.Month > remindDate.Month)
                    {
                        return true;
                    }
                    else
                    {
                        if (DateTime.Now.Month < remindMonth)
                        {
                            Debug.Log("Month >>> 提醒时间或者当前系统时间有问题");
                        }
                        else
                        {
                            if (DateTime.Now.Day > remindDay)
                            {
                                return true;
                            }
                            else
                            {
                                if (DateTime.Now.Day < remindDay)
                                {
                                    Debug.Log("Day >>> 提醒时间或者当前系统时间有问题");
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
}