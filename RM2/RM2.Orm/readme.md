# MyMiniOrm
一个简单的Orm实现，项目作业的数据访问工具层。
---
### 约定
数据实体对象继承自 IEntity 接口，该接口定义了一个 int 类型的Id属性。

```
public interface IEntity
{
    int Id { get; set; }
}

// 数据实体类
public class Student : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ClazzId { get; set; }
    public Clazz Clazz { get; set; }

    // 更新时忽略该属性
    [MyColumn(UpdateIgnore = true)]
    public DateTime CreateAt { get; set; }
}

public class Clazz : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

## 用法

实例化对象：

```
var db = new MyDb("DataSource=.;Database=Test;USER ID=sa;Password=1234");
```
### 查询单个实体：
```
var student = db.Load<Student>(1);

var student = db.Load<Student>(s => s.Name == "张三");
```
### 查询多个实体：
```
var student = db.Fetch<Student>();

var student = db.PageList<T>(2, 10, out var recordCount, s => s.Name.Contains("张三"), s=>s.Name);
```
### Fluent 查询
```
var query = db.Query<Student>()
    .Include(s => s.Clazz)
    .Where(s => s.CreateAt > DateTime.Today.AddDays(-1))
    .OrderBy(s => s.Clazz.Id)
    .ThenOrderByDesc(s => s.Name);

var student = query.FirstOrDefault();
var students = query.ToList();
var students2 = query.ToPageList(2, 2, out var recordCount);
```
### 插入
```
var student = new Student
{
    Name = "张三",
    ClazzId = 1,
    CreateAt = DateTime.Now
};
db.Insert(student);    // 会将新产生的Id赋值到student.Id属性
Console.WriteLine($"{student.Id}");

// 批量插入

var students = new List<Student>
{
    new Student {Name = "张三", ClazzId = 1, CreateAt = DateTime.Now},
    new Student {Name = "李四", ClazzId = 1, CreateAt = DateTime.Now},
    new Student {Name = "王五", ClazzId = 1, CreateAt = DateTime.Now},
    new Student {Name = "赵六", ClazzId = 1, CreateAt = DateTime.Now}
};

db.Insert(students);

foreach (var stu in students)
{
    Console.WriteLine($"{stu.Id}-{stu.Name}");
}
```

### 更新
```
var student = db.Load<Student>(1);
student.Name = student.Name + "修改过";
var result = db.Update(student);

// 批量更新
var students = db.Fetch<Student>(s => s.Id > 1);
foreach (var student in students)
{
    student.Name += student.Name + "批量修改过";
}
var count = db.Update(students);
Console.WriteLine($"修改了 {count} 行");
```

### 更新-注意，以下内容未经过测试
```
db.Update<Student>(1, DbKvs.New().Add("Name", "张三"));

var student = db.Load<Student>(1);
student.Name = student.Name + "测试修改";
student.ClazzId = 2;

// 注意，下面方法传入的是属性名而不列名
var count = db.Update<Student>(student, new[] {"Name", "ClazzId"});
var count2 = db.UpdateIgnore<Student>(student, new[] {"CreateAt"});

db.Update<Student>(DbKvs.New().Add("ClazzId", 2), s => s.ClazzId == 1);
```

### 删除
```
db.Delete<Student>(1);

db.Delete<Student>(new[] {1,2,3});
```

暂时这么多，后继功能将陆续添加进来。注意：此项目仅用于作业内部交流，代码为经过比较严格的测试，可能会有坑，慎入。
