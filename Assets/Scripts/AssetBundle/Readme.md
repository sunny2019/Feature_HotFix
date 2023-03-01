TODO:
1. 增加多选打包平台，对应平台的AB包在对应的文件夹内
2. AssetBundleConfig.bytes 文件位置调整
3. AssetBundle的加载路径调整;
4. 整理ObjecManager AssetBundleManager ResourceManager
5. 类对象池增加接口，对所有类对象池管理的类继承接口，在回收时对类对象进行重置



内容：
1.引用计数
2.预加载
3.同步及异步加载
4.双向链表缓存
5.AB包管理
6.资源管理
7.对象管理(类对象池及对象池)