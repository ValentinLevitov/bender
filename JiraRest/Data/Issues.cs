using System;

#nullable disable
namespace JiraRest.Data
{
    public class User
    {
        public string self { get; set; }
        public string name { get; set; }
        public string emailAddress { get; set; }
        public object avatarUrls { get; set; }
        public string displayName { get; set; }
        public bool active { get; set; }
    }

    public class Project
    {
        public string self { get; set; }
        public int id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public object avatarUrls { get; set; }
    }

    public class Issuetype
    {
        public string self { get; set; }
        public int id { get; set; }
        public string description { get; set; }
        public string iconUrl { get; set; }
        public string name { get; set; }
        public bool subtask { get; set; }
    }

    public class Component
    {
        public string self { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Vote
    {
        public string self { get; set; }
        public int votes { get; set; }
        public bool hasVoted { get; set; }
    }

    public class Watches
    {
        public string self { get; set; }
        public int watchCount { get; set; }
        public bool isWatching { get; set; }
    }

    public class Progress
    {
        public int progress { get; set; }
        public int total { get; set; }
        public double percent { get; set; }
    }

    public class SimpleCustomField
    {
        public string self { get; set; }
        public string value { get; set; }
        public int id { get; set; }
    }

    public class Version
    {
        public string self { get; set; }
        public int id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public bool archived { get; set; }
        public bool released { get; set; }
        public DateTime? releaseDate { get; set; }
    }

    public class Resolution
    {
        public string self { get; set; }
        public int id { get; set; }
        public string description { get; set; }
        public string name { get; set; }
    }

    public class Priority
    {
        public string self { get; set; }
        public string iconUrl { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Status
    {
        public string self { get; set; }
        public string desription { get; set; }
        public string iconUrl { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Fields
    {
        public string summary { get; set; }
        public string description { get; set; }
        public Issuetype issuetype { get; set; }
        public int? timespent { get; set; }
        public User reporter { get; set; }
        public User creator { get; set; }
        public DateTime created { get; set; }
        public Project project { get; set; }

        public DateTime? lastViewed { get; set; }
        public Component[] components { get; set; }
        public int? timeoriginalestimate { get; set; }
        public Vote votes { get; set; }
        public DateTime? resolutiondate { get; set; }
        public DateTime? duedate { get; set; }
        public Watches watches { get; set; }
        public Progress progress { get; set; }

        public DateTime? updated { get; set; }
        public Priority priority { get; set; }
        public object subtasks { get; set; }
        public Status status { get; set; }
        public Resolution resolution { get; set; }
        public string[] labels { get; set; }
        public long? workratio { get; set; }
        public Progress aggregateprogress { get; set; }
        public Version[] fixVersions { get; set; }
        public int? aggregatetimeoriginalestimate { get; set; }
        public User assignee { get; set; }
        public int? aggregatetimeestimate { get; set; }
        public Version[] versions { get; set; }
        public int? aggregatetimespent { get; set; }
    }

    public class Issue
    {
        public string expand { get; set; }
        public int id { get; set; }
        public string self { get; set; }
        public string key { get; set; }
        public Fields fields { get; set; }
        public ChangelogResponse changelog { get; set; }
    }

    public class IssuesResponse
    {
        public string expand { get; set; }
        public int startAt { get; set; }
        public int maxResults { get; set; }
        public int total { get; set; }
        public Issue[] issues { get; set; }
    }
}
