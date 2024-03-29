﻿using IBApi;

namespace Rx.IB2.Services.IbApiHandlers;

public partial class IbApiHandler {
    public void currentTime(long time) {
        throw new NotImplementedException();
    }

    public void tickEFP(
        int tickerId, int tickType, double basisPoints, string formattedBasisPoints,
        double impliedFuture,
        int holdDays, string futureLastTradeDate, double dividendImpact, double dividendsToLastTradeDate
    ) {
        throw new NotImplementedException();
    }

    public void deltaNeutralValidation(int reqId, DeltaNeutralContract deltaNeutralContract) {
        throw new NotImplementedException();
    }

    public void tickSnapshotEnd(int tickerId) {
        throw new NotImplementedException();
    }

    public void accountSummary(int reqId, string account, string tag, string value, string currency) {
        throw new NotImplementedException();
    }

    public void accountSummaryEnd(int reqId) {
        throw new NotImplementedException();
    }

    public void bondContractDetails(int reqId, ContractDetails contract) {
        throw new NotImplementedException();
    }

    public void fundamentalData(int reqId, string data) {
        throw new NotImplementedException();
    }

    public void updateMktDepth(int tickerId, int position, int operation, int side, double price, decimal size) {
        throw new NotImplementedException();
    }

    public void updateMktDepthL2(
        int tickerId, int position, string marketMaker, int operation, int side, double price,
        decimal size, bool isSmartDepth
    ) {
        throw new NotImplementedException();
    }

    public void updateNewsBulletin(int msgId, int msgType, string message, string origExchange) {
        throw new NotImplementedException();
    }

    public void position(string account, Contract contract, decimal pos, double avgCost) {
        throw new NotImplementedException();
    }

    public void positionEnd() {
        throw new NotImplementedException();
    }

    public void realtimeBar(
        int reqId, long date, double open, double high, double low, double close, decimal volume,
        decimal wap, int count
    ) {
        throw new NotImplementedException();
    }

    public void scannerParameters(string xml) {
        throw new NotImplementedException();
    }

    public void scannerData(
        int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark,
        string projection, string legsStr
    ) {
        throw new NotImplementedException();
    }

    public void scannerDataEnd(int reqId) {
        throw new NotImplementedException();
    }

    public void receiveFA(int faDataType, string faXmlData) {
        throw new NotImplementedException();
    }

    public void verifyMessageAPI(string apiData) {
        throw new NotImplementedException();
    }

    public void verifyCompleted(bool isSuccessful, string errorText) {
        throw new NotImplementedException();
    }

    public void verifyAndAuthMessageAPI(string apiData, string xyzChallenge) {
        throw new NotImplementedException();
    }

    public void verifyAndAuthCompleted(bool isSuccessful, string errorText) {
        throw new NotImplementedException();
    }

    public void displayGroupList(int reqId, string groups) {
        throw new NotImplementedException();
    }

    public void displayGroupUpdated(int reqId, string contractInfo) {
        throw new NotImplementedException();
    }

    public void positionMulti(
        int requestId, string account, string modelCode, Contract contract, decimal pos, double avgCost
    ) {
        throw new NotImplementedException();
    }

    public void positionMultiEnd(int requestId) {
        throw new NotImplementedException();
    }

    public void accountUpdateMulti(
        int requestId, string account, string modelCode, string key, string value, string currency
    ) {
        throw new NotImplementedException();
    }

    public void accountUpdateMultiEnd(int requestId) {
        throw new NotImplementedException();
    }

    public void softDollarTiers(int reqId, SoftDollarTier[] tiers) {
        throw new NotImplementedException();
    }

    public void familyCodes(FamilyCode[] familyCodes) {
        throw new NotImplementedException();
    }

    public void symbolSamples(int reqId, ContractDescription[] contractDescriptions) {
        throw new NotImplementedException();
    }

    public void mktDepthExchanges(DepthMktDataDescription[] depthMktDataDescriptions) {
        throw new NotImplementedException();
    }

    public void tickNews(
        int tickerId, long timeStamp, string providerCode, string articleId, string headline,
        string extraData
    ) {
        throw new NotImplementedException();
    }

    public void smartComponents(int reqId, Dictionary<int, KeyValuePair<string, char>> theMap) {
        throw new NotImplementedException();
    }

    public void newsProviders(NewsProvider[] newsProviders) {
        throw new NotImplementedException();
    }

    public void newsArticle(int requestId, int articleType, string articleText) {
        throw new NotImplementedException();
    }

    public void historicalNews(int requestId, string time, string providerCode, string articleId, string headline) {
        throw new NotImplementedException();
    }

    public void historicalNewsEnd(int requestId, bool hasMore) {
        throw new NotImplementedException();
    }

    public void headTimestamp(int reqId, string headTimestamp) {
        throw new NotImplementedException();
    }

    public void histogramData(int reqId, HistogramEntry[] data) {
        throw new NotImplementedException();
    }

    public void rerouteMktDataReq(int reqId, int conId, string exchange) {
        throw new NotImplementedException();
    }

    public void rerouteMktDepthReq(int reqId, int conId, string exchange) {
        throw new NotImplementedException();
    }

    public void marketRule(int marketRuleId, PriceIncrement[] priceIncrements) {
        throw new NotImplementedException();
    }

    public void historicalTicks(int reqId, HistoricalTick[] ticks, bool done) {
        throw new NotImplementedException();
    }

    public void historicalTicksBidAsk(int reqId, HistoricalTickBidAsk[] ticks, bool done) {
        throw new NotImplementedException();
    }

    public void historicalTicksLast(int reqId, HistoricalTickLast[] ticks, bool done) {
        throw new NotImplementedException();
    }

    public void tickByTickAllLast(
        int reqId, int tickType, long time, double price, decimal size,
        TickAttribLast tickAttribLast, string exchange, string specialConditions
    ) {
        throw new NotImplementedException();
    }

    public void tickByTickBidAsk(
        int reqId, long time, double bidPrice, double askPrice, decimal bidSize,
        decimal askSize, TickAttribBidAsk tickAttribBidAsk
    ) {
        throw new NotImplementedException();
    }

    public void tickByTickMidPoint(int reqId, long time, double midPoint) {
        throw new NotImplementedException();
    }

    public void replaceFAEnd(int reqId, string text) {
        throw new NotImplementedException();
    }

    public void wshMetaData(int reqId, string dataJson) {
        throw new NotImplementedException();
    }

    public void wshEventData(int reqId, string dataJson) {
        throw new NotImplementedException();
    }

    public void historicalSchedule(
        int reqId, string startDateTime, string endDateTime, string timeZone,
        HistoricalSession[] sessions
    ) {
        throw new NotImplementedException();
    }

    public void userInfo(int reqId, string whiteBrandingId) {
        throw new NotImplementedException();
    }
}