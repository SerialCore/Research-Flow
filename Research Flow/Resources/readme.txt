消息通知方式：
Toast，只用于后台通知
Header(Event)，适用于前台通知，且用户能注意到
InAppNotification，只用于捕获异常
Dialog，适用于操作限制

List process:
list of rsssources stored as one file
list of feeds stored as one file which cannot be modified
list of notes stored as files, reorderd by name

Todo:
目前的同步机制不能满足远程删除，要么使用数据库管理文件，要么在解压时对照被删除文件。
Quartz.NET作业调度
WebView存储浏览记录，保存在Log文件夹里
RSS列表可自定义顺序，包括手动拖拽，或者一键排序
Feed内容要包含时间，这样可以通知新Feed，并可以给用户呈现出一个热点、时间图
数据库要存哪些数据？适合存储小数据，不适合大段文字和复杂格式以及复杂的继承关系。
注册后台任务
爬虫的界面、流、存储
自定义关键词，项目管理
关键词的图表示
朋友圈界面，聊天界面，联系人界面
主页的平板化界面
论文管理
应用内搜索，文件遍历
收集用户信息
登录IP，保存在Log里
Url搜索引擎
哪些文件需要同步，扩展名
需不需要log文件，记录什么？
优化数据存储结构、加密方式，选择导出到设备
不同数据源的学习方法是不同的，网页爬虫，Feed统计
How-tos
x3C对接，3.0