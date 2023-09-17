import pandasnet
from datetime import datetime, date, timedelta, time

from LibForTests import DatetimeNet as dtnet


def test_datetime():
    now = datetime.now()
    result = dtnet.DateTimePassThrough(now)
    assert result == now

def test_date():
    today = datetime.now().date()
    result = dtnet.DateTimePassThrough(today)

    # Convert date to datetime
    today = datetime.combine(today, datetime.min.time())
    assert result == today

def test_timedelta():
    td = timedelta(days=1, hours=2, minutes=12, seconds=24, milliseconds=123)
    result = dtnet.TimeSpanPassThrough(td)

    assert result == td

def test_time():
    now = datetime.now().time()
    result = dtnet.TimeSpanPassThrough(now)

    # convert time to timedelta
    now = datetime.combine(date.min, now) - datetime.min
    assert result == now