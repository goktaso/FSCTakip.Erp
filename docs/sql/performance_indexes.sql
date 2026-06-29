-- ============================================================
-- FSC Takip ERP — Performans Index'leri
-- Aşama 1: Hemen uygula (veri az olsa da etki anında görülür)
-- SSMS'te FscErpDb üzerinde çalıştır.
-- ============================================================

USE FscErpDb;
GO

-- 1. FscSerial → Lot join (AnaOzet, RawMaterial her sorguda kullanır)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FscSerials_LotId' AND object_id = OBJECT_ID('FscSerials'))
    CREATE INDEX IX_FscSerials_LotId
        ON FscSerials (LotId)
        INCLUDE (InitialWeight, CurrentWeight, SerialNo);
GO

-- 2. FscSerial → Aktif stok filtresi (WHERE CurrentWeight > 0)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FscSerials_CurrentWeight_Filtered')
    CREATE INDEX IX_FscSerials_CurrentWeight_Filtered
        ON FscSerials (CurrentWeight)
        INCLUDE (LotId, SerialNo, InitialWeight)
        WHERE CurrentWeight > 0;
GO

-- 3. ProductionDetails → Serial join (tüketim/fire hesabı her sayfada)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductionDetails_FscSerialId')
    CREATE INDEX IX_ProductionDetails_FscSerialId
        ON ProductionDetails (FscSerialId)
        INCLUDE (ConsumedWeight, WasteWeight, WorkOrderId, ProductionDate);
GO

-- 4. FscLot → YM dönüşüm sorgusu (SourceSerialId IS NOT NULL filtresi)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FscLots_SourceSerialId_Filtered')
    CREATE INDEX IX_FscLots_SourceSerialId_Filtered
        ON FscLots (SourceSerialId)
        INCLUDE (ProductId, FscTypeId, ArrivalDate, PartiNo, ConversionFireKg)
        WHERE SourceSerialId IS NOT NULL;
GO

-- 5. FscLot → Ham malzeme sorgusu (SourceSerialId IS NULL + tarih filtresi)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FscLots_ArrivalDate_Ham')
    CREATE INDEX IX_FscLots_ArrivalDate_Ham
        ON FscLots (ArrivalDate DESC)
        INCLUDE (ProductId, SupplierId, FscTypeId, PartiNo)
        WHERE SourceSerialId IS NULL;
GO

-- 6. WorkOrder → ProductionDetail join
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductionDetails_WorkOrderId')
    CREATE INDEX IX_ProductionDetails_WorkOrderId
        ON ProductionDetails (WorkOrderId)
        INCLUDE (FscSerialId, ConsumedWeight, WasteWeight, ProducedQuantity);
GO

PRINT 'Index''ler başarıyla oluşturuldu.';
GO

-- Sonrası: Sorgu süresini ölç
-- SET STATISTICS TIME ON;
-- SELECT ... (AnaOzet sorgusu)
-- SET STATISTICS TIME OFF;
