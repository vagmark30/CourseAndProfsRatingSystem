﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourseAndProfsPersistence.Helpers;

namespace CourseAndProfsPersistence.Models
{
  public class CourseType : CaPEntity<long>
  {
    public string Name { get; set; }
  }
}
