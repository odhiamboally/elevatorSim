﻿namespace ES.Domain.Entities;

public class Floor
{
    public int Id { get; set; }
    public int PeopleCount { get; set; }
    public Queue<int> RequestQueue { get; set; } = [];
}
