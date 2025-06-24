Module CacheEditProduct
    Public cacheProductUpdateCompletion As Boolean = False
    Public cacheProductID As String
    Public cacheProductName As String
    Public cacheProductCode As String
    Public cacheCategoryID As Int64
    Public cacheSubCategoryID As Int64
    Public cacheSupplierID As Int64
    Public cacheBrandID As Int64
    Public cacheWarehouseID As Integer
    Public cacheProductImage As String
    Public cacheMeasurementUnit As String
    Public cacheProductVariation As Boolean
    Public cacheProductDescription As String
    Public cacheSellingPrice As Double
    Public cacheBuyingPrice As Double
    Public cacheStockUnit As Integer
    Public cacheAlertQuantity As Integer
    Public cacheSerialNumbers As New List(Of String)
    Public cacheSerialID As New List(Of Integer)
End Module

Module CacheDeleteProduct
    Public cacheDeleteProductID As String
    Public cacheDeleteProductName As String
End Module
